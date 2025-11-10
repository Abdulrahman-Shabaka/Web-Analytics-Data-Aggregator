namespace AnalyticsAggregator.Core.Models;

public class PageSpeedInsightsRecord
{
    public string Date { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public decimal PerformanceScore { get; set; }
    public int LCP_ms { get; set; }
}

