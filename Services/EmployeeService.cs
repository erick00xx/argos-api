using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Enums;
using ArgosApi.Models;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArgosApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Define el orden de las columnas del CSV. Modifica aquí para agregar/quitar columnas.
    private static class CsvColumns
    {
        public const int DocumentType = 0;
        public const int Document = 1;
        public const int EnrolledId = 2;
        public const int FileCode = 3;
        public const int FirstName = 4;
        public const int LastName = 5;
        public const int Profession = 6;
        public const int ClockName = 7;
        public const int BirthDate = 8;
        public const int Gender = 9;
        public const int OriginCountry = 10;
        public const int PersonalEmail = 11;
        public const int CorporateEmail = 12;
        public const int ContractStartDate = 13;
        public const int AddressLine1 = 14;
        public const int AddressLine2 = 15;
        public const int City = 16;
        public const int HomePhone = 17;
        public const int MobilePhone = 18;
        public const int Department = 19;
        public const int Branch = 20;
        public const int CompanyOrAliasTaxId = 21;
    }

    public async Task<Result<EmployeeCsvImportResultDto>> ImportFromCsvAsync(IFormFile file, Guid companyId, Guid? userId)
    {
        if (file == null || file.Length == 0)
            return Result<EmployeeCsvImportResultDto>.Fail("El archivo CSV es requerido.", 400);

        var result = new EmployeeCsvImportResultDto();

        var companyTaxId = await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.TaxId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(companyTaxId))
            return Result<EmployeeCsvImportResultDto>.Fail("Compañía no encontrada.", 404);

        // Precargamos catálogos en memoria para evitar una consulta por fila.
        var branches = await _context.Branches
            .AsNoTracking()
            .Where(b => b.CompanyId == companyId)
            .ToListAsync();

        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.CompanyId == companyId)
            .ToListAsync();

        var aliases = await _context.CompanyAliases
            .AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .Select(a => new { a.Id, a.TaxId })
            .ToListAsync();

        var employeeKeys = await _context.Employees
            .AsNoTracking()
            .Where(e => e.CompanyId == companyId)
            .Select(e => new { e.Id, e.Document })
            .ToListAsync();

        // Diccionarios para búsqueda rápida por nombre normalizado.
        var branchesByName = branches
            .GroupBy(x => Normalize(x.Name), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var departmentsByName = departments
            .GroupBy(x => Normalize(x.Name), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var aliasesByTaxId = aliases
            .GroupBy(x => Normalize(x.TaxId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var employeesByDocument = employeeKeys
            .GroupBy(x => Normalize(x.Document), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var lines = content
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count <= 1)
            return Result<EmployeeCsvImportResultDto>.Fail("El CSV no tiene filas para procesar.", 400);

        // Saltamos el encabezado (línea 0), procesamos desde la línea 1.
        result.TotalRows = lines.Count - 1;

        for (var i = 1; i < lines.Count; i++)
        {
            var rowNumber = i + 1; // Número real de línea en el CSV.
            var cols = SplitCsvLine(lines[i]);

            // Extraemos valores por posición ordinal.
            var documentTypeText = GetValue(cols, CsvColumns.DocumentType);
            var document = GetValue(cols, CsvColumns.Document);
            var enrolledId = GetValue(cols, CsvColumns.EnrolledId);
            var fileCode = GetValue(cols, CsvColumns.FileCode);
            var firstName = GetValue(cols, CsvColumns.FirstName);
            var lastName = GetValue(cols, CsvColumns.LastName);
            var profession = GetValue(cols, CsvColumns.Profession);
            var clockNameFromCsv = GetValue(cols, CsvColumns.ClockName);
            var birthDateText = GetValue(cols, CsvColumns.BirthDate);
            var gender = GetValue(cols, CsvColumns.Gender);
            var originCountry = GetValue(cols, CsvColumns.OriginCountry);
            var personalEmail = GetValue(cols, CsvColumns.PersonalEmail);
            var corporateEmail = GetValue(cols, CsvColumns.CorporateEmail);
            var contractStartDateText = GetValue(cols, CsvColumns.ContractStartDate);
            var addressLine1 = GetValue(cols, CsvColumns.AddressLine1);
            var addressLine2 = GetValue(cols, CsvColumns.AddressLine2);
            var city = GetValue(cols, CsvColumns.City);
            var homePhone = GetValue(cols, CsvColumns.HomePhone);
            var mobilePhone = GetValue(cols, CsvColumns.MobilePhone);
            var departmentName = GetValue(cols, CsvColumns.Department);
            var branchName = GetValue(cols, CsvColumns.Branch);
            var companyOrAliasTaxId = GetValue(cols, CsvColumns.CompanyOrAliasTaxId);

            // Validar campos obligatorios.
            if (string.IsNullOrWhiteSpace(documentTypeText) || string.IsNullOrWhiteSpace(document) ||
                string.IsNullOrWhiteSpace(enrolledId) ||
                string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(branchName) || string.IsNullOrWhiteSpace(departmentName) ||
                string.IsNullOrWhiteSpace(companyOrAliasTaxId))
            {
                AddRowError(result, rowNumber, "La fila no tiene todos los campos obligatorios.");
                continue;
            }

            if (!TryParseDocumentType(documentTypeText, out var documentType))
            {
                AddRowError(result, rowNumber, $"Tipo de documento inválido: '{documentTypeText}'.");
                continue;
            }

            // Validar RUC de compañía o alias.
            var normalizedTaxId = Normalize(companyOrAliasTaxId);
            var isCompanyTaxId = Normalize(companyTaxId) == normalizedTaxId;
            Guid? aliasId = null;

            if (!isCompanyTaxId)
            {
                if (!aliasesByTaxId.TryGetValue(normalizedTaxId, out var alias))
                {
                    AddRowError(result, rowNumber, $"RUC no encontrado en compañía ni en alias: '{companyOrAliasTaxId}'.");
                    continue;
                }

                aliasId = alias.Id;
            }

            // Buscar sucursal por nombre normalizado.
            var normalizedBranchName = Normalize(branchName);
            if (!branchesByName.TryGetValue(normalizedBranchName, out var branch))
            {
                AddRowError(result, rowNumber, $"Sucursal no encontrada: '{branchName}'.");
                continue;
            }

            // Buscar departamento por nombre normalizado.
            var normalizedDepartmentName = Normalize(departmentName);
            if (!departmentsByName.TryGetValue(normalizedDepartmentName, out var department))
            {
                AddRowError(result, rowNumber, $"Departamento no encontrado: '{departmentName}'.");
                continue;
            }

            // Parsear fechas usando timezone de sucursal.
            if (!TryParseNullableDateWithTimeZone(birthDateText, branch.TimeZone, out var birthDateUtc))
            {
                AddRowError(result, rowNumber, $"BirthDate inválida: '{birthDateText}'.");
                continue;
            }

            if (!TryParseNullableDateWithTimeZone(contractStartDateText, branch.TimeZone, out var contractStartDateUtc))
            {
                AddRowError(result, rowNumber, $"ContractStartDate inválida: '{contractStartDateText}'.");
                continue;
            }

            if (!ValidateMaxLength(document, 30) ||
                !ValidateMaxLength(enrolledId, 10) ||
                !ValidateMaxLength(fileCode, 30) ||
                !ValidateMaxLength(firstName, 100) ||
                !ValidateMaxLength(lastName, 100) ||
                !ValidateMaxLength(profession, 120) ||
                !ValidateMaxLength(gender, 20) ||
                !ValidateMaxLength(originCountry, 100) ||
                !ValidateMaxLength(personalEmail, 150) ||
                !ValidateMaxLength(corporateEmail, 150) ||
                !ValidateMaxLength(addressLine1, 200) ||
                !ValidateMaxLength(addressLine2, 200) ||
                !ValidateMaxLength(city, 100) ||
                !ValidateMaxLength(homePhone, 30) ||
                !ValidateMaxLength(mobilePhone, 30))
            {
                AddRowError(result, rowNumber, "La fila contiene valores que exceden la longitud máxima permitida.");
                continue;
            }

            var generatedClockName = BuildClockName(firstName, lastName, 20);
            var finalClockName = string.IsNullOrWhiteSpace(clockNameFromCsv)
                ? generatedClockName
                : Truncate(clockNameFromCsv, 100);

            var normalizedDocument = Normalize(document);
            if (employeesByDocument.TryGetValue(normalizedDocument, out var existingEmployeeId))
            {
                // Actualizar sin materializar entidad completa (evita cast de enums legacy).
                await _context.Employees
                    .Where(e => e.Id == existingEmployeeId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(e => e.DocumentType, documentType)
                        .SetProperty(e => e.Document, document)
                        .SetProperty(e => e.EnrolledId, enrolledId)
                        .SetProperty(e => e.FileCode, string.IsNullOrWhiteSpace(fileCode) ? null : fileCode)
                        .SetProperty(e => e.FirstName, firstName)
                        .SetProperty(e => e.LastName, lastName)
                        .SetProperty(e => e.Profession, string.IsNullOrWhiteSpace(profession) ? null : profession)
                        .SetProperty(e => e.ClockName, finalClockName)
                        .SetProperty(e => e.Gender, string.IsNullOrWhiteSpace(gender) ? null : gender)
                        .SetProperty(e => e.OriginCountry, string.IsNullOrWhiteSpace(originCountry) ? null : originCountry)
                        .SetProperty(e => e.PersonalEmail, string.IsNullOrWhiteSpace(personalEmail) ? null : personalEmail)
                        .SetProperty(e => e.CorporateEmail, string.IsNullOrWhiteSpace(corporateEmail) ? null : corporateEmail)
                        .SetProperty(e => e.AddressLine1, string.IsNullOrWhiteSpace(addressLine1) ? null : addressLine1)
                        .SetProperty(e => e.AddressLine2, string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2)
                        .SetProperty(e => e.City, string.IsNullOrWhiteSpace(city) ? null : city)
                        .SetProperty(e => e.HomePhone, string.IsNullOrWhiteSpace(homePhone) ? null : homePhone)
                        .SetProperty(e => e.MobilePhone, string.IsNullOrWhiteSpace(mobilePhone) ? null : mobilePhone)
                        .SetProperty(e => e.BranchId, branch.Id)
                        .SetProperty(e => e.DepartmentId, department.Id)
                        .SetProperty(e => e.CompanyId, companyId)
                        .SetProperty(e => e.AliasId, aliasId)
                        .SetProperty(e => e.BirthDate, birthDateUtc)
                        .SetProperty(e => e.ContractStartDate, contractStartDateUtc)
                        .SetProperty(e => e.UpdatedAt, DateTime.UtcNow)
                        .SetProperty(e => e.UpdatedBy, userId));

                result.Updated++;
            }
            else
            {
                // Insertar nuevo empleado.
                var employee = new Employee
                {
                    CompanyId = companyId,
                    DepartmentId = department.Id,
                    BranchId = branch.Id,
                    AliasId = aliasId,
                    DocumentType = documentType,
                    Document = document,
                    EnrolledId = enrolledId,
                    FileCode = string.IsNullOrWhiteSpace(fileCode) ? null : fileCode,
                    PasswordHash = document, // Contraseña inicial temporal igual al documento.
                    FirstName = firstName,
                    LastName = lastName,
                    Profession = string.IsNullOrWhiteSpace(profession) ? null : profession,
                    ClockName = generatedClockName,
                    BirthDate = birthDateUtc,
                    Gender = string.IsNullOrWhiteSpace(gender) ? null : gender,
                    OriginCountry = string.IsNullOrWhiteSpace(originCountry) ? null : originCountry,
                    PersonalEmail = string.IsNullOrWhiteSpace(personalEmail) ? null : personalEmail,
                    CorporateEmail = string.IsNullOrWhiteSpace(corporateEmail) ? null : corporateEmail,
                    AddressLine1 = string.IsNullOrWhiteSpace(addressLine1) ? null : addressLine1,
                    AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2,
                    City = string.IsNullOrWhiteSpace(city) ? null : city,
                    HomePhone = string.IsNullOrWhiteSpace(homePhone) ? null : homePhone,
                    MobilePhone = string.IsNullOrWhiteSpace(mobilePhone) ? null : mobilePhone,
                    ContractStartDate = contractStartDateUtc,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Employees.Add(employee);
                employeesByDocument[normalizedDocument] = employee.Id;
                result.Inserted++;
            }
        }

        await _context.SaveChangesAsync();

        return Result<EmployeeCsvImportResultDto>.Ok(result);
    }

    private static List<string> SplitCsvLine(string line)
    {
        // Separa por ';' y normaliza espacios.
        return line.Split(';').Select(x => x.Trim()).ToList();
    }

    private static string? GetValue(List<string> cols, int index)
    {
        // Obtiene el valor por posición ordinal, retorna null si está fuera de rango o vacío.
        if (index < 0 || index >= cols.Count)
            return null;

        var value = cols[index]?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void AddRowError(EmployeeCsvImportResultDto result, int rowNumber, string message)
    {
        result.Errors.Add(new EmployeeCsvImportErrorDto
        {
            RowNumber = rowNumber,
            Message = message
        });

        _logger.LogWarning("Importación CSV de empleados - Fila {RowNumber}: {Message}", rowNumber, message);
    }

    private static bool TryParseDateWithTimeZone(string? value, string? timeZoneId, out DateTime utcDate)
    {
        utcDate = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!DateTime.TryParse(value, out var parsed))
            return false;

        if (parsed.Kind == DateTimeKind.Utc)
        {
            utcDate = parsed;
            return true;
        }

        if (parsed.Kind == DateTimeKind.Local)
        {
            utcDate = parsed.ToUniversalTime();
            return true;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                utcDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                return true;
            }

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                utcDate = TimeZoneInfo.ConvertTimeToUtc(parsed, tz);
                return true;
            }
            catch
            {
                utcDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseNullableDateWithTimeZone(string? value, string? timeZoneId, out DateTime? utcDate)
    {
        utcDate = null;

        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!TryParseDateWithTimeZone(value, timeZoneId, out var parsedUtc))
            return false;

        utcDate = parsedUtc;
        return true;
    }

    private static bool ValidateMaxLength(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return value.Trim().Length <= max;
    }

    private static string BuildClockName(string firstName, string lastName, int maxLength)
    {
        var fullName = $"{firstName} {lastName}".Trim();
        return Truncate(fullName, maxLength);
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static bool TryParseDocumentType(string? value, out DocumentType documentType)
    {
        documentType = DocumentType.Unknown;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var normalized = Normalize(value);
        return normalized switch
        {
            "dni" => SetDocumentType(DocumentType.DNI, out documentType),
            "documentonacionaldeidentidad" => SetDocumentType(DocumentType.DNI, out documentType),
            "passport" => SetDocumentType(DocumentType.Passport, out documentType),
            "pasaporte" => SetDocumentType(DocumentType.Passport, out documentType),
            "unknown" => SetDocumentType(DocumentType.Unknown, out documentType),
            "desconocido" => SetDocumentType(DocumentType.Unknown, out documentType),
            _ => false
        };
    }

    private static bool SetDocumentType(DocumentType value, out DocumentType output)
    {
        output = value;
        return true;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-').ToArray());
    }

    public async Task<PagedResult<List<EmployeeDto>>> GetPagedAsync(Guid companyId, EmployeeRequestDto request, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IQueryable<Employee>? baseQuery = _context.Employees
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId)
                .Where(e =>
                (string.IsNullOrWhiteSpace(request.EnrolledId) || e.EnrolledId.Contains(request.EnrolledId)) &&
                (string.IsNullOrWhiteSpace(request.Document) || e.Document.Contains(request.Document)) &&
                (string.IsNullOrWhiteSpace(request.FirstName) || e.FirstName.ToLower().Contains(request.FirstName.ToLower())) &&
                (string.IsNullOrWhiteSpace(request.LastName) || e.LastName.ToLower().Contains(request.LastName.ToLower())) &&
                (string.IsNullOrWhiteSpace(request.BranchName) || e.Branch.Name.ToLower().Contains(request.BranchName.ToLower())) &&
                (string.IsNullOrWhiteSpace(request.DepartmentName) || e.Department.Name.ToLower().Contains(request.DepartmentName.ToLower())) &&
                (!request.Status.HasValue || e.IsActive == request.Status.Value)
                );

            var totalRecords = await baseQuery.CountAsync();

            var employees = await baseQuery
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EnrolledId = e.EnrolledId,
                    DocumentType = e.DocumentType.ToString(),
                    DocumentNumber = e.Document,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    BranchName = e.Branch.Name,
                    DepartmentName = e.Department.Name,
                    IsActive = e.IsActive
                }).ToListAsync();

            return PagedResult<List<EmployeeDto>>.Ok(employees, pageNumber, pageSize, totalRecords);
        }
        catch (System.Exception)
        {

            throw;
        }
    }
}
