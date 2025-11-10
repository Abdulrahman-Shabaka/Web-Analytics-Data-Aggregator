namespace AnalyticsAggregator.Core.Models;

public class CombinedAnalyticsRecord
{
    public string Page { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Views { get; set; }
    public decimal? PerformanceScore { get; set; }
    public int? LCP_ms { get; set; }
}

