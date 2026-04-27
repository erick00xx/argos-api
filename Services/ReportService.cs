using System.Globalization;
using System.Text;
using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Enums;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class ReportService : IReportService
{
	private const string ReportTimeZoneId = "America/Lima";
	private readonly ApplicationDbContext _context;

	public ReportService(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<Result<byte[]>> ExportAttendancesCsvAsync(Guid companyId, AttendanceReportFilterDto? filter = null)
	{
		var query = _context.Attendances
			.AsNoTracking()
			.Where(a => a.Employee.CompanyId == companyId);

		if (filter?.EmployeeId.HasValue == true)
			query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);

		if (filter?.StartDate.HasValue == true)
		{
			var startUtc = ConvertPeruDateBoundaryToUtc(filter.StartDate.Value.Date, isEndBoundary: false);
			query = query.Where(a => a.PunchDateTime >= startUtc);
		}

		if (filter?.EndDate.HasValue == true)
		{
			var endUtc = ConvertPeruDateBoundaryToUtc(filter.EndDate.Value.Date.AddDays(1), isEndBoundary: true);
			query = query.Where(a => a.PunchDateTime < endUtc);
		}

		var rows = await query
			.OrderBy(a => a.Employee.LastName)
			.ThenBy(a => a.Employee.FirstName)
			.ThenBy(a => a.PunchDateTime)
			.Select(a => new AttendanceReportRowDto
			{
				Document = a.Employee.Document,
				Code = a.Employee.EnrolledId,
				FileCode = a.Employee.FileCode ?? string.Empty,
				Name = a.Employee.LastName + " " + a.Employee.FirstName,
				Rut = a.Employee.AliasId != null ? a.Employee.Alias!.TaxId : a.Employee.Company.TaxId,
				CompanyName = a.Employee.AliasId != null ? a.Employee.Alias!.Name : a.Employee.Company.CompanyName,
				BranchName = a.Employee.Branch.Name,
				DepartmentName = a.Employee.Department.Name,
				Date = a.PunchDateTime,
				ShiftAssigned = string.Empty,       // Pa implemntar en el futuro
				ScheduleOrEventName = string.Empty,
				BreakMinutes = string.Empty,
				StartTime = string.Empty,
				EndTime = string.Empty,
				EntryTime = string.Empty,
				ExitTime = string.Empty,
				PunchType = a.PunchType,
			})
			.ToListAsync();

		if (rows.Count == 0)
			return Result<byte[]>.Fail("No hay marcaciones para exportar.", 404);

		var csv = BuildCsv(rows);
		var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
		return Result<byte[]>.Ok(bytes);
	}

	private static string BuildCsv(List<AttendanceReportRowDto> rows)
	{
		var headers = new[]
		{
			"Documento",
			"Codigo en Huellero",
			"Ficha",
			"Nombre",
			"RUT",
			"Compañía",
			"Sucursal",
			"Departamento",
			"Fecha",
			"Hora Entrada",
			"Hora salida",
			"Turno asignado",
			"Nombre horario/evento",
			"Minutos descanso",
			"Hora inicio",
			"Hora término",
		};

		var builder = new StringBuilder();
		builder.AppendLine(string.Join(";", headers));

		foreach (var row in rows)
		{
			var peruDateTime = ConvertUtcToPeruTime(row.Date);
			var entryTime = row.PunchType == AttendancePunchType.CheckIn ? peruDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
			var exitTime = row.PunchType == AttendancePunchType.CheckOut ? peruDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;

			var values = new[]
			{
				row.Document,
				row.Code,
				row.FileCode,
				row.Rut,
				row.Name,
				row.CompanyName,
				row.BranchName,
				row.DepartmentName,
				peruDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
				entryTime,
				exitTime,
				row.ShiftAssigned,
				row.ScheduleOrEventName,
				row.BreakMinutes,
				row.StartTime,
				row.EndTime,
			};

			builder.AppendLine(string.Join(";", values.Select(EscapeCsvValue)));
		}

		return builder.ToString();
	}

	private static string EscapeCsvValue(string? value)
	{
		value ??= string.Empty;

		var mustQuote = value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
		var escaped = value.Replace("\"", "\"\"");

		return mustQuote ? $"\"{escaped}\"" : escaped;
	}

	private static DateTime ConvertUtcToPeruTime(DateTime utcDateTime)
	{
		if (utcDateTime.Kind == DateTimeKind.Unspecified)
			utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

		if (utcDateTime.Kind != DateTimeKind.Utc)
			utcDateTime = utcDateTime.ToUniversalTime();

		var timeZone = ResolveReportTimeZone();
		return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
	}

	private static DateTime ConvertPeruDateBoundaryToUtc(DateTime peruBoundaryDateTime, bool isEndBoundary)
	{
		var timeZone = ResolveReportTimeZone();
		var unspecified = DateTime.SpecifyKind(peruBoundaryDateTime, DateTimeKind.Unspecified);
		var utc = TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
		return isEndBoundary ? utc : utc;
	}

	private static TimeZoneInfo ResolveReportTimeZone()
	{
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById(ReportTimeZoneId);
		}
		catch (TimeZoneNotFoundException)
		{
			return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
		}
	}
}
