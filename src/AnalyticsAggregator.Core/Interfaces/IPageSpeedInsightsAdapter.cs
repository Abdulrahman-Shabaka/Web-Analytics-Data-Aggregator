using AnalyticsAggregator.Core.Models;

namespace AnalyticsAggregator.Core.Interfaces;

public interface IPageSpeedInsightsAdapter
{
    Task<List<PageSpeedInsightsRecord>> ReadDataAsync(string filePath);
}

