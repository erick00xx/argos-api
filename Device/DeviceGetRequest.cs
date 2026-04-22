namespace ArgosApi.Dtos;

public class DeviceGetRequest
{
    public string SN { get; set; } = null!;
    public string? INFO { get; set; }
}