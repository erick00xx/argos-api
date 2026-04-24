namespace ArgosApi.Dtos;

public class EmployeeCsvImportResultDto
{
    public int TotalRows { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int ErrorsCount => Errors.Count;
    public List<EmployeeCsvImportErrorDto> Errors { get; set; } = new();
}

public class EmployeeCsvImportErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}