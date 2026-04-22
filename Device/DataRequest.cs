namespace ArgosApi.Dtos;

public class DataRequest
{
    public string SN { get; set; } = null!;
    public string table { get; set; } = null!;
    // Solo registros de Operaciones
    public string? OpStamp { get; set; }
    // Solo registros de Asistencias
    public string? Stamp { get; set; }
}