using AnalyticsAggregator.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsAggregator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IDataIngestionService _ingestionService;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(IDataIngestionService ingestionService, ILogger<IngestionController> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartIngestion()
    {
        try
        {
            await _ingestionService.IngestDataAsync();
            return Ok(new { Message = "Data ingestion started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data ingestion");
            return StatusCode(500, new { Error = "Failed to start data ingestion", Details = ex.Message });
        }
    }
}

