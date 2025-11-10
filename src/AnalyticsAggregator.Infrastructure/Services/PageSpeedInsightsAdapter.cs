using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Core.Models;
using System.Text.Json;

namespace AnalyticsAggregator.Infrastructure.Services;

public class PageSpeedInsightsAdapter : IPageSpeedInsightsAdapter
{
    public async Task<List<PageSpeedInsightsRecord>> ReadDataAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"PSI data file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var records = JsonSerializer.Deserialize<List<PageSpeedInsightsRecord>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return records ?? new List<PageSpeedInsightsRecord>();
    }
}

