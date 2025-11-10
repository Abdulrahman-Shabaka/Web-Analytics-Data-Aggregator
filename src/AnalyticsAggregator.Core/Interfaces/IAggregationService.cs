using AnalyticsAggregator.Core.Entities;

namespace AnalyticsAggregator.Core.Interfaces;

public interface IAggregationService
{
    Task AggregateDailyStatsAsync(RawData rawData);
}

