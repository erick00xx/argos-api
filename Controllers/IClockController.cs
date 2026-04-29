using System;
using ArgosApi.Dtos;
using ArgosApi.Handlers;
using ArgosApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IClockController : ControllerBase
{

    private readonly ClockDataProcessor _clockDataProcessor;
    private readonly IDeviceCommandService _clockService;

    public IClockController(ClockDataProcessor clockDataProcessor, IDeviceCommandService deviceCommandService)
    {
        _clockDataProcessor = clockDataProcessor;
        _clockService = deviceCommandService;
    }
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Content("OK", "text/plain");
    }

    [HttpGet("cdata")]
    public async Task<IActionResult> InitDevice([FromQuery] InitDeviceRequest request) // Initialize device.
    {
        return Content("OK", "text/plain");
    }
    [HttpGet("getrequest")] // Send commands.
    public async Task<IActionResult> GetRequest([FromQuery] DeviceGetRequest request)
    {

        var response = await _clockService.ExecuteCommandAsync(request.SN);
        return Content(response, "text/plain");
    }

    [HttpPost("cdata")] // Receive attendance data.
    public async Task<IActionResult> GetData([FromQuery] DataRequest request)
    {
        using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: false);
        var body = await reader.ReadToEndAsync();

        var success = await _clockDataProcessor.ProcessClockDataAsync(request.SN, request.table, body);

        if (success)
            return Content("OK", "text/plain");

        return StatusCode(500, "Error processing data");
    }

    [HttpPost("devicecmd")] // Receive command execution result.
    public async Task<IActionResult> ReceiveCommandResult([FromQuery] CommandResultRequest request)
    {
        using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: false);
        var body = await reader.ReadToEndAsync();
        var success = await _clockService.ProcessDeviceCommandResultsAsync(request.SN, body);

        if (success)
            return Content("OK", "text/plain");

        return StatusCode(500, "Error processing command result");
    }

}

public class CommandResultRequest
{
    public string SN { get; set; } = string.Empty;
}