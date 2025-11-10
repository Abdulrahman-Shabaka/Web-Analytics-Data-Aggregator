namespace AnalyticsAggregator.Core.Entities;

public class RawData
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Page { get; set; } = string.Empty;
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Views { get; set; }
    public decimal? PerformanceScore { get; set; }
    public int? LCPms { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

