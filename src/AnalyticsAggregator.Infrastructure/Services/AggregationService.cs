using AnalyticsAggregator.Core.Entities;
using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnalyticsAggregator.Infrastructure.Services;

public class AggregationService : IAggregationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AggregationService> _logger;

    public AggregationService(ApplicationDbContext context, ILogger<AggregationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AggregateDailyStatsAsync(RawData rawData)
    {
        var date = rawData.Date.Date;

        var existingStats = await _context.DailyStats
            .FirstOrDefaultAsync(s => s.Date == date);

        if (existingStats != null)
        {
            // Update existing stats
            var allRawDataForDate = await _context.RawData
                .Where(r => r.Date.Date == date)
                .ToListAsync();

            existingStats.TotalUsers = allRawDataForDate.Sum(r => r.Users);
            existingStats.TotalSessions = allRawDataForDate.Sum(r => r.Sessions);
            existingStats.TotalViews = allRawDataForDate.Sum(r => r.Views);
            
            var performanceScores = allRawDataForDate
                .Where(r => r.PerformanceScore.HasValue)
                .Select(r => r.PerformanceScore!.Value)
                .ToList();
            
            existingStats.AvgPerformance = performanceScores.Any() 
                ? performanceScores.Average() 
                : 0;
            existingStats.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updated daily stats for {Date}", date);
        }
        else
        {
            // Create new stats
            var allRawDataForDate = await _context.RawData
                .Where(r => r.Date.Date == date)
                .ToListAsync();

            var performanceScores = allRawDataForDate
                .Where(r => r.PerformanceScore.HasValue)
                .Select(r => r.PerformanceScore!.Value)
                .ToList();

            var newStats = new DailyStats
            {
                Date = date,
                TotalUsers = allRawDataForDate.Sum(r => r.Users),
                TotalSessions = allRawDataForDate.Sum(r => r.Sessions),
                TotalViews = allRawDataForDate.Sum(r => r.Views),
                AvgPerformance = performanceScores.Any() ? performanceScores.Average() : 0,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.DailyStats.Add(newStats);
            _logger.LogInformation("Created new daily stats for {Date}", date);
        }

        await _context.SaveChangesAsync();
    }
}

