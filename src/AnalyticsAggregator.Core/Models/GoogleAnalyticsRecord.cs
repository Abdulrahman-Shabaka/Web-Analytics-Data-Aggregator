namespace AnalyticsAggregator.Core.Models;

public class GoogleAnalyticsRecord
{
    public string Date { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Views { get; set; }
}

