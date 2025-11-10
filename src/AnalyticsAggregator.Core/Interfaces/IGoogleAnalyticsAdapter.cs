using AnalyticsAggregator.Core.Models;

namespace AnalyticsAggregator.Core.Interfaces;

public interface IGoogleAnalyticsAdapter
{
    Task<List<GoogleAnalyticsRecord>> ReadDataAsync(string filePath);
}

