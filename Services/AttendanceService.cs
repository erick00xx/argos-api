using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Enums;
using ArgosApi.Models;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;

    public AttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> SaveMultipleAttendancesAsync(List<AttendanceLogDto> attendances)
    {
        if (attendances == null || !attendances.Any())
            return false;

        if (attendances.Count == 1)
            return await SaveSingleAttendanceAsync(attendances.First());

        var pins = attendances
            .Select(a => a.Pin?.Trim())
            .Where(pin => !string.IsNullOrWhiteSpace(pin))
            .Distinct()
            .ToList();

        var serialNumbers = attendances
            .Select(a => a.Sn?.Trim())
            .Where(sn => !string.IsNullOrWhiteSpace(sn))
            .Distinct()
            .ToList();

        var employeeByPin = await _context.Employees
            .AsNoTracking()
            .Where(e => pins.Contains(e.EnrolledId))
            .ToDictionaryAsync(e => e.EnrolledId, e => e.Id);

        var deviceBySerial = await _context.Devices
            .AsNoTracking()
            .Where(d => serialNumbers.Contains(d.SerialNumber))
            .Select(d => new { d.SerialNumber, d.Id, d.TimeZone })
            .ToDictionaryAsync(d => d.SerialNumber, d => new { d.Id, d.TimeZone });

        var recordsToInsert = new List<Attendance>();

        foreach (var dto in attendances)
        {
            if (!employeeByPin.TryGetValue(dto.Pin.Trim(), out var employeeId))
                continue;

            Guid? deviceId = null;
            string? deviceTimeZone = null;
            if (!string.IsNullOrWhiteSpace(dto.Sn) && deviceBySerial.TryGetValue(dto.Sn.Trim(), out var resolvedDevice))
            {
                deviceId = resolvedDevice.Id;
                deviceTimeZone = resolvedDevice.TimeZone;
            }

            recordsToInsert.Add(MapToAttendance(dto, employeeId, deviceId, deviceTimeZone));
        }

        if (!recordsToInsert.Any())
            return false;

        _context.Attendances.AddRange(recordsToInsert);


        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> SaveSingleAttendanceAsync(AttendanceLogDto attendance)
    {
        if (attendance == null)
            return false;

        var pin = attendance.Pin?.Trim();
        if (string.IsNullOrWhiteSpace(pin))
            return false;

        var employeeId = await _context.Employees
            .AsNoTracking()
            .Where(e => e.EnrolledId == pin)
            .Select(e => (Guid?)e.Id)
            .FirstOrDefaultAsync();

        if (!employeeId.HasValue)
            return false;

        Guid? deviceId = null;
        string? deviceTimeZone = null;
        var serialNumber = attendance.Sn?.Trim();
        if (!string.IsNullOrWhiteSpace(serialNumber))
        {
            var resolvedDevice = await _context.Devices
                .AsNoTracking()
                .Where(d => d.SerialNumber == serialNumber)
                .Select(d => new { Id = (Guid?)d.Id, d.TimeZone })
                .FirstOrDefaultAsync();

            deviceId = resolvedDevice?.Id;
            deviceTimeZone = resolvedDevice?.TimeZone;
        }

        var newAttendance = MapToAttendance(attendance, employeeId.Value, deviceId, deviceTimeZone);

        _context.Attendances.Add(newAttendance);
        await _context.SaveChangesAsync();

        return true;
    }

    private static Attendance MapToAttendance(AttendanceLogDto dto, Guid employeeId, Guid? deviceId, string? deviceTimeZone)
    {
        return new Attendance
        {
            EmployeeId = employeeId,
            DeviceId = deviceId,
            PunchDateTime = ConvertToUtcFromDeviceTimeZone(dto.PunchDateTime, deviceTimeZone),
            PunchType = Enum.Parse<AttendancePunchType>(dto.PunchType),
            Method = Enum.Parse<AttendanceMethod>(dto.Method),
            Source = AttendanceSource.Device,
            RawClockUserId = dto.Pin,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static DateTime ConvertToUtcFromDeviceTimeZone(DateTime dateTime, string? timeZoneId)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        // La fecha que llega del reloj es hora local del reloj (sin offset).
        var localClockTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

        if (string.IsNullOrWhiteSpace(timeZoneId))
            return DateTime.SpecifyKind(localClockTime, DateTimeKind.Utc);

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(localClockTime, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateTime.SpecifyKind(localClockTime, DateTimeKind.Utc);
        }
        catch (InvalidTimeZoneException)
        {
            return DateTime.SpecifyKind(localClockTime, DateTimeKind.Utc);
        }
    }

    public async Task<PagedResult<List<AttendanceDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? employeeId = null)
    {
        try
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalRecords = await _context.Attendances
                .AsNoTracking()
                .Where(a => !employeeId.HasValue || a.EmployeeId == employeeId.Value).CountAsync();
            
            var skip = (pageNumber - 1) * pageSize;

            var attendances = await _context.Attendances
                .AsNoTracking()
                .Where(a => !employeeId.HasValue || a.EmployeeId == employeeId.Value)
                .OrderByDescending(a => a.PunchDateTime)
                .Skip(skip)
                .Take(pageSize)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    DeviceName = a.Device!.Name,
                })
                .ToListAsync();

            return PagedResult<List<AttendanceDto>>.Ok(attendances, pageNumber, pageSize, totalRecords);

        }
        catch (Exception ex)
        {
            return PagedResult<List<AttendanceDto>>.Fail("An error occurred while retrieving attendances: " + ex.Message);
        }
    }
}