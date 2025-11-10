using AnalyticsAggregator.Core.Models;

namespace AnalyticsAggregator.Core.Interfaces;

public interface IDataCombiner
{
    List<CombinedAnalyticsRecord> Combine(
        List<GoogleAnalyticsRecord> gaData,
        List<PageSpeedInsightsRecord> psiData);
}

