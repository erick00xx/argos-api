using System.Security.Cryptography;
using System.Text;
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
    private readonly string _attendanceChecksumSecret;

    public AttendanceService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _attendanceChecksumSecret = configuration["AttendanceChecksum:Secret"]
            ?? configuration["Jwt:Key"]
            ?? "Argos_Default_Attendance_Checksum_Secret_ChangeMe";
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
            .Select(d => new { d.SerialNumber, d.Id, d.TimeZone, BranchAddressLine1 = d.Branch.AddressLine1 })
            .ToDictionaryAsync(d => d.SerialNumber, d => new { d.Id, d.TimeZone, d.BranchAddressLine1 });

        var recordsToInsert = new List<Attendance>();

        foreach (var dto in attendances)
        {
            if (!employeeByPin.TryGetValue(dto.Pin.Trim(), out var employeeId))
                continue;

            Guid? deviceId = null;
            string? deviceTimeZone = null;
            string? location = null;
            if (!string.IsNullOrWhiteSpace(dto.Sn) && deviceBySerial.TryGetValue(dto.Sn.Trim(), out var resolvedDevice))
            {
                deviceId = resolvedDevice.Id;
                deviceTimeZone = resolvedDevice.TimeZone;
                location = resolvedDevice.BranchAddressLine1;
            }

            recordsToInsert.Add(MapToAttendance(dto, employeeId, deviceId, deviceTimeZone, location));
        }

        if (!recordsToInsert.Any())
            return false;

        _context.Attendances.AddRange(recordsToInsert);


        if (await _context.SaveChangesAsync() > 0)
            return true;

        return false;
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
        string? location = null;
        var serialNumber = attendance.Sn?.Trim();
        if (!string.IsNullOrWhiteSpace(serialNumber))
        {
            var resolvedDevice = await _context.Devices
                .AsNoTracking()
                .Where(d => d.SerialNumber == serialNumber)
                .Select(d => new { Id = (Guid?)d.Id, d.TimeZone, BranchAddressLine1 = d.Branch.AddressLine1 })
                .FirstOrDefaultAsync();

            deviceId = resolvedDevice?.Id;
            deviceTimeZone = resolvedDevice?.TimeZone;
            location = resolvedDevice?.BranchAddressLine1;
        }

        var newAttendance = MapToAttendance(attendance, employeeId.Value, deviceId, deviceTimeZone, location);

        _context.Attendances.Add(newAttendance);

        if (await _context.SaveChangesAsync() > 0)
            return true;

        return false;
    }

    private Attendance MapToAttendance(AttendanceLogDto dto, Guid employeeId, Guid? deviceId, string? deviceTimeZone, string? location)
    {
        var punchDateTimeUtc = ConvertToUtcFromDeviceTimeZone(dto.PunchDateTime, deviceTimeZone);
        var punchType = Enum.Parse<AttendancePunchType>(dto.PunchType);
        var method = Enum.Parse<AttendanceMethod>(dto.Method);
        var source = AttendanceSource.Device;

        return new Attendance
        {
            EmployeeId = employeeId,
            DeviceId = deviceId,
            PunchDateTime = punchDateTimeUtc,
            PunchType = punchType,
            Method = method,
            Source = source,
            Location = string.IsNullOrWhiteSpace(location) ? null : location,
            RawClockUserId = dto.Pin,
            Checksum = BuildAttendanceChecksum(
                employeeId,
                punchDateTimeUtc,
                punchType,
                method,
                source,
                deviceId,
                dto.Pin,
                location),
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

    public async Task<PagedResult<List<AttendanceDto>>> GetPagedAsync(
        int pageNumber = 1, int pageSize = 10,
        Guid? employeeId = null,
        DateTime? startDate = null, DateTime? endDate = null,
        AttendancePunchType? punchType = null,
        AttendanceMethod? method = null,
        AttendanceSource? source = null,
        string? deviceName = null)
    {
        try
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalRecords = await _context.Attendances
                .AsNoTracking()
                .Where(a => !employeeId.HasValue || a.EmployeeId == employeeId.Value).CountAsync();

            var skip = (pageNumber - 1) * pageSize;


            IQueryable<Attendance>? query = _context.Attendances
                .AsNoTracking();

            if (startDate.HasValue)
                query = query.Where(a => a.PunchDateTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.PunchDateTime <= endDate.Value);
            if (punchType.HasValue)
                query = query.Where(a => a.PunchType == punchType.Value);
            if (method.HasValue)
                query = query.Where(a => a.Method == method.Value);
            if (source.HasValue)
                query = query.Where(a => a.Source == source.Value);
            if (!string.IsNullOrWhiteSpace(deviceName))
                query = query.Where(a => a.Device != null && a.Device.Name.Contains(deviceName));


            var attendances = await query
                .Where(a => !employeeId.HasValue || a.EmployeeId == employeeId.Value)
                .OrderByDescending(a => a.PunchDateTime)
                .Skip(skip)
                .Take(pageSize)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    FullName = a.Employee.FirstName + " " + a.Employee.LastName,
                    Timestamp = a.PunchDateTime,
                    Type = a.PunchType,
                    Method = a.Method,
                    Source = a.Source,
                    EmployeeId = a.EmployeeId,
                    DeviceName = a.Device!.Name
                })
                .ToListAsync();

            return PagedResult<List<AttendanceDto>>.Ok(attendances, pageNumber, pageSize, totalRecords);

        }
        catch (Exception ex)
        {
            return PagedResult<List<AttendanceDto>>.Fail("An error occurred while retrieving attendances: " + ex.Message);
        }
    }

    public async Task<Result<bool>> MarkAttendanceWebAsync(Guid employeeId, int punchType)
    {

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
            return Result<bool>.Fail("Employee not found", 404);

        if (!employee.attWebAllowed)
            return Result<bool>.Fail("Employee is not allowed to mark attendance from web", 403);

        var nowUtc = DateTime.UtcNow;
        var parsedPunchType = (AttendancePunchType)punchType;

        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            PunchDateTime = nowUtc,
            PunchType = parsedPunchType,
            Method = AttendanceMethod.Unknown,
            Source = AttendanceSource.Web,
            Checksum = BuildAttendanceChecksum(
                employeeId,
                nowUtc,
                parsedPunchType,
                AttendanceMethod.Unknown,
                AttendanceSource.Web,
                null,
                null,
                null)
        };
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<AttendanceDetailDto>> GetAttendanceDetailAsync(Guid attendanceId, Guid employeeId)
    {
        var row = await _context.Attendances
            .Where(a => a.Id == attendanceId)
            .Select(a => new
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                FullName = a.Employee.FirstName + " " + a.Employee.LastName,
                Timestamp = a.PunchDateTime,
                Type = a.PunchType,
                Method = a.Method,
                Source = a.Source,
                DeviceName = a.Device != null ? a.Device.Name : "N/A",
                IsAttValid = a.IsValid,
                EnrolledId = a.Employee.EnrolledId,
                Location = a.Location,
                DocumentType = a.Employee.DocumentType.ToString(),
                EmployeeDocument = a.Employee.Document,
                IsEmployeeTracked = a.Employee.IsAttendanceTracked,
                Checksum = a.Checksum ?? string.Empty,
                RawClockUserId = a.RawClockUserId,
                DeviceId = a.DeviceId,
                CompanyInfo = new AttendanceCompanyDto
                {
                    CompanyName = a.Employee.AliasId != null ? a.Employee.Alias!.Name : a.Employee.Company.CompanyName,
                    TaxType = a.Employee.AliasId != null ? a.Employee.Alias!.TaxType.ToString() : a.Employee.Company.TaxType.ToString(),
                    CompanyTaxId = a.Employee.AliasId != null ? a.Employee.Alias!.TaxId : a.Employee.Company.TaxId,
                    BranchName = a.Employee.Branch.Name,
                    DepartmentName = a.Employee.Department.Name,
                }
            }).FirstOrDefaultAsync();

        if (row == null)
            return Result<AttendanceDetailDto>.Fail("Attendance record not found or not accessible", 404);

        var expectedChecksum = BuildAttendanceChecksum(
            row.EmployeeId,
            row.Timestamp,
            row.Type,
            row.Method,
            row.Source,
            row.DeviceId,
            row.RawClockUserId,
            row.Location);

        var response = new AttendanceDetailDto
        {
            Id = row.Id,
            EmployeeId = row.EmployeeId,
            FullName = row.FullName,
            Timestamp = row.Timestamp,
            Type = row.Type,
            Method = row.Method,
            Source = row.Source,
            DeviceName = row.DeviceName,
            IsAttValid = row.IsAttValid,
            EnrolledId = row.EnrolledId,
            Location = row.Location ?? string.Empty,
            DocumentType = row.DocumentType,
            EmployeeDocument = row.EmployeeDocument,
            IsEmployeeTracked = row.IsEmployeeTracked,
            Checksum = row.Checksum,
            isChecksumValid = !string.IsNullOrWhiteSpace(row.Checksum) && row.Checksum == expectedChecksum,
            CompanyInfo = row.CompanyInfo
        };

        return Result<AttendanceDetailDto>.Ok(response);
    }

    private string BuildAttendanceChecksum(
        Guid employeeId,
        DateTime punchDateTimeUtc,
        AttendancePunchType punchType,
        AttendanceMethod method,
        AttendanceSource source,
        Guid? deviceId,
        string? rawClockUserId,
        string? location)
    {
        // Patrón fijo del payload para que el hash sea consistente en tamaño y estructura.
        var payload = string.Join("|", new[]
        {
            "v1",
            employeeId.ToString("N"),
            punchDateTimeUtc.ToUniversalTime().ToString("O"),
            ((int)punchType).ToString(),
            ((int)method).ToString(),
            ((int)source).ToString(),
            deviceId?.ToString("N") ?? string.Empty,
            rawClockUserId?.Trim() ?? string.Empty,
            location?.Trim() ?? string.Empty
            // isValid ? "1" : "0"
        });

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_attendanceChecksumSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        // 64 caracteres hex (longitud fija).
        return Convert.ToHexString(hashBytes);
    }

}