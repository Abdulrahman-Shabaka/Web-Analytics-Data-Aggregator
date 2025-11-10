using Microsoft.AspNetCore.Mvc;

namespace AnalyticsAggregator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "Analytics Aggregator API"
        });
    }

    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            memory = new
            {
                workingSet = process.WorkingSet64,
                workingSetMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2)
            },
            cpu = new
            {
                processorTime = process.TotalProcessorTime.TotalMilliseconds
            },
            threads = process.Threads.Count,
            uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime()
        });
    }
}

