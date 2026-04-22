using System;
using ArgosApi.Dtos;
using ArgosApi.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IClockController : ControllerBase
{

    private readonly ClockDataProcessor _clockDataProcessor;

    public IClockController(ClockDataProcessor clockDataProcessor)
    {
        _clockDataProcessor = clockDataProcessor;
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

        
        return Content("OK", "text/plain");
    }

    [HttpPost("cdata")] // Receive attendance data.
    public async Task<IActionResult> GetData([FromQuery] DataRequest request)
    {
        using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: false);
        var body = await reader.ReadToEndAsync();

        await _clockDataProcessor.ProcessClockDataAsync(request.SN, request.table, body);

        return Content("OK", "text/plain");
    }

}