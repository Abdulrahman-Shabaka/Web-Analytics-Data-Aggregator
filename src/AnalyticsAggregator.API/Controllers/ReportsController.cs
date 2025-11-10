using AnalyticsAggregator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsAggregator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var stats = await _context.DailyStats.ToListAsync();

        var overview = new
        {
            TotalUsers = stats.Sum(s => s.TotalUsers),
            TotalSessions = stats.Sum(s => s.TotalSessions),
            TotalViews = stats.Sum(s => s.TotalViews),
            AveragePerformance = stats.Any() ? stats.Average(s => s.AvgPerformance) : 0,
            DateRange = stats.Any() ? new
            {
                From = stats.Min(s => s.Date),
                To = stats.Max(s => s.Date)
            } : null
        };

        return Ok(overview);
    }

    [HttpGet("pages")]
    public async Task<IActionResult> GetPages()
    {
        var rawData = await _context.RawData.ToListAsync();

        var pageStats = rawData
            .GroupBy(r => r.Page)
            .Select(g => new
            {
                Page = g.Key,
                TotalUsers = g.Sum(r => r.Users),
                TotalSessions = g.Sum(r => r.Sessions),
                TotalViews = g.Sum(r => r.Views),
                AveragePerformance = g.Where(r => r.PerformanceScore.HasValue)
                    .Select(r => r.PerformanceScore!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLCP = g.Where(r => r.LCPms.HasValue)
                    .Select(r => r.LCPms!.Value)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        return Ok(pageStats);
    }
}

