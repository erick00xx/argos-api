using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Enums;
using ArgosApi.Models;
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

        var employeeByPin = await _context.ClockProfiles
            .AsNoTracking()
            .Where(cp => pins.Contains(cp.EnrolledId))
            .ToDictionaryAsync(cp => cp.EnrolledId, cp => cp.EmployeeId);

        var deviceBySerial = await _context.Devices
            .AsNoTracking()
            .Where(d => serialNumbers.Contains(d.SerialNumber))
            .ToDictionaryAsync(d => d.SerialNumber, d => d.Id);

        var recordsToInsert = new List<Attendance>();

        foreach (var dto in attendances)
        {
            if (!employeeByPin.TryGetValue(dto.Pin.Trim(), out var employeeId))
                continue;

            Guid? deviceId = null;
            if (!string.IsNullOrWhiteSpace(dto.Sn) && deviceBySerial.TryGetValue(dto.Sn.Trim(), out var resolvedDeviceId))
                deviceId = resolvedDeviceId;

            recordsToInsert.Add(MapToAttendance(dto, employeeId, deviceId));
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

        var employeeId = await _context.ClockProfiles
            .AsNoTracking()
            .Where(cp => cp.EnrolledId == pin)
            .Select(cp => (Guid?)cp.EmployeeId)
            .FirstOrDefaultAsync();

        if (!employeeId.HasValue)
            return false;

        Guid? deviceId = null;
        var serialNumber = attendance.Sn?.Trim();
        if (!string.IsNullOrWhiteSpace(serialNumber))
        {
            deviceId = await _context.Devices
                .AsNoTracking()
                .Where(d => d.SerialNumber == serialNumber)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync();
        }

        var newAttendance = MapToAttendance(attendance, employeeId.Value, deviceId);

        _context.Attendances.Add(newAttendance);
        await _context.SaveChangesAsync();

        return true;
    }

    private static Attendance MapToAttendance(AttendanceLogDto dto, Guid employeeId, Guid? deviceId)
    {
        return new Attendance
        {
            EmployeeId = employeeId,
            DeviceId = deviceId,
            PunchDateTime = dto.PunchDateTime,
            PunchType = Enum.Parse<AttendancePunchType>(dto.PunchType),
            Method = Enum.Parse<AttendanceMethod>(dto.Method),
            Source = AttendanceSource.Device,
            RawClockUserId = dto.Pin,
            CreatedAt = DateTime.UtcNow
        };
    }
}