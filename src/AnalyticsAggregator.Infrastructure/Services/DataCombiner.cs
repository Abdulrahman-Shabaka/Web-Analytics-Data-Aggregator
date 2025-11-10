using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Core.Models;

namespace AnalyticsAggregator.Infrastructure.Services;

public class DataCombiner : IDataCombiner
{
    public List<CombinedAnalyticsRecord> Combine(
        List<GoogleAnalyticsRecord> gaData,
        List<PageSpeedInsightsRecord> psiData)
    {
        var combined = new List<CombinedAnalyticsRecord>();

        foreach (var ga in gaData)
        {
            var psi = psiData.FirstOrDefault(p => p.Date == ga.Date && p.Page == ga.Page);

            combined.Add(new CombinedAnalyticsRecord
            {
                Page = ga.Page,
                Date = ga.Date,
                Users = ga.Users,
                Sessions = ga.Sessions,
                Views = ga.Views,
                PerformanceScore = psi?.PerformanceScore,
                LCP_ms = psi?.LCP_ms
            });
        }

        return combined;
    }
}

