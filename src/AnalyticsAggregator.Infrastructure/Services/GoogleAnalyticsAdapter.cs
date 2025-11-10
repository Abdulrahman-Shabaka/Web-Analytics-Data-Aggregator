using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Core.Models;
using System.Text.Json;

namespace AnalyticsAggregator.Infrastructure.Services;

public class GoogleAnalyticsAdapter : IGoogleAnalyticsAdapter
{
    public async Task<List<GoogleAnalyticsRecord>> ReadDataAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"GA data file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var records = JsonSerializer.Deserialize<List<GoogleAnalyticsRecord>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return records ?? new List<GoogleAnalyticsRecord>();
    }
}

