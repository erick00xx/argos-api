namespace ArgosApi.Services;

public interface IDeviceCommandService
{
    // Task<bool> ProcessClockDataAsync(string sn, string table, string body);
    Task<bool> ProcessDeviceCommandResultsAsync(string sn, string body);
    Task<string> ExecuteCommandAsync(string sn);
}