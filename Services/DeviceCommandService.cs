
using ArgosApi.Data;
using ArgosApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class DeviceCommandService : IDeviceCommandService
{
    ApplicationDbContext _context;
    private readonly ILogger<DeviceCommandService> _logger;

    public DeviceCommandService(ApplicationDbContext context, ILogger<DeviceCommandService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ProcessDeviceCommandResultsAsync(string sn, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Empty body received for device command results (SN: {sn})", sn);
            return false;
        }
        var lines = body.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Parse all lines first and collect results
        var parsedResults = new List<(int? Id, string ReturnCode, string Cmd, string Raw)>();

        foreach (var rawLine in lines)
        {
            try
            {
                var pairs = rawLine.Split('&', StringSplitOptions.RemoveEmptyEntries);
                int? id = null;
                string returnCode = string.Empty;
                string cmd = string.Empty;

                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length != 2) continue;
                    var key = kv[0].Trim();
                    var val = kv[1].Trim();

                    if (string.Equals(key, "ID", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(val, out var parsedId)) id = parsedId;
                        else _logger.LogWarning("Unable to parse ID as int for SN={sn}: {val}. Raw: {raw}", sn, val, rawLine);
                    }
                    else if (string.Equals(key, "Return", StringComparison.OrdinalIgnoreCase)) returnCode = val;
                    else if (string.Equals(key, "CMD", StringComparison.OrdinalIgnoreCase)) cmd = val;
                }

                if (!id.HasValue && string.IsNullOrEmpty(returnCode) && string.IsNullOrEmpty(cmd))
                {
                    _logger.LogWarning("No key/value pairs parsed from device command result line (SN: {sn}): {line}", sn, rawLine);
                    continue;
                }

                parsedResults.Add((id, returnCode, cmd, rawLine));
                _logger.LogInformation("Parsed device command result (SN: {sn}): ID={id}, Return={ret}, CMD={cmd}", sn, id?.ToString() ?? "(none)", returnCode, cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing device command result line for SN={sn}: {line}", sn, rawLine);
            }
        }

        var ids = parsedResults.Where(r => r.Id.HasValue).Select(r => r.Id!.Value).Distinct().ToList();

        if (!ids.Any())
        {
            // Nothing with an ID to update, just return true after logging
            return true;
        }

        // Fetch all matching DeviceCommands in a single query
        var commands = await _context.DeviceCommands
            .Include(dc => dc.Device)
            .Where(dc => dc.Device.SerialNumber == sn && ids.Contains(dc.CommandNumber))
            .ToListAsync();

        if (!commands.Any())
        {
            _logger.LogWarning("No DeviceCommands found for SN={sn} and IDs: {ids}", sn, string.Join(',', ids));
            return true;
        }

        var commandByNumber = commands.ToDictionary(c => c.CommandNumber);
        var anyProcessed = false;

        // Update each command based on parsed results
        foreach (var res in parsedResults)
        {
            if (!res.Id.HasValue)
            {
                _logger.LogWarning("Parsed device command result without ID for SN={sn}: {raw}", sn, res.Raw);
                continue;
            }

            if (commandByNumber.TryGetValue(res.Id.Value, out var deviceCommand))
            {
                deviceCommand.Status = res.ReturnCode == "0" ? "success" : "failed";
                deviceCommand.ExecutedAt = DateTime.UtcNow;
                deviceCommand.ReturnCode = res.ReturnCode;
                anyProcessed = true;
                _logger.LogInformation("Updated DeviceCommand Id={id} (SN={sn}) to status={status}", deviceCommand.Id, sn, deviceCommand.Status);
            }
            else
            {
                _logger.LogWarning("No DeviceCommand entity found for SN={sn} and CommandNumber={id}. Raw: {raw}", sn, res.Id.Value, res.Raw);
            }
        }

        if (anyProcessed)
        {
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<string> ExecuteCommandAsync(string sn)
    {
        var commandToSend = await _context.DeviceCommands
            .Where(dc =>
                dc.Device.SerialNumber == sn &&
                dc.Status == "pending")
            .OrderBy(dc => dc.CreatedAt)
            .FirstOrDefaultAsync();

        if (commandToSend != null)
        {
            _logger.LogInformation($"Enviando comando al dispositivo {sn}: {commandToSend}");
            Console.WriteLine($"Enviando comando al dispositivo {sn}: {commandToSend}");
            commandToSend.Status = "sent";
            commandToSend.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return commandToSend.CommandText;
        }
        else
        {
            return "OK"; // Respuesta por defecto si no hay comandos pendientes
        }
    }
}