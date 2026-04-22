namespace ArgosApi.Dtos;

public class InitDeviceRequest
{
    public string SN { get; set; } = null!;
    public string? options { get; set; }
    public string? language { get; set; }
    public string? pushver { get; set; }
    public string? PushOptionsFlag { get; set; }
    
}