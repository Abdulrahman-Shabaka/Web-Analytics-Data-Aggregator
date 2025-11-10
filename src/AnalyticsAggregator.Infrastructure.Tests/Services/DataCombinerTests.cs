using AnalyticsAggregator.Core.Models;
using AnalyticsAggregator.Infrastructure.Services;
using Xunit;

namespace AnalyticsAggregator.Infrastructure.Tests.Services;

public class DataCombinerTests
{
    [Fact]
    public void Combine_ShouldMatchRecordsByDateAndPage()
    {
        // Arrange
        var combiner = new DataCombiner();
        var gaData = new List<GoogleAnalyticsRecord>
        {
            new() { Date = "2025-10-20", Page = "/home", Users = 120, Sessions = 150, Views = 310 }
        };
        var psiData = new List<PageSpeedInsightsRecord>
        {
            new() { Date = "2025-10-20", Page = "/home", PerformanceScore = 0.9m, LCP_ms = 2100 }
        };

        // Act
        var result = combiner.Combine(gaData, psiData);

        // Assert
        Assert.Single(result);
        Assert.Equal("/home", result[0].Page);
        Assert.Equal(120, result[0].Users);
        Assert.Equal(0.9m, result[0].PerformanceScore);
        Assert.Equal(2100, result[0].LCP_ms);
    }

    [Fact]
    public void Combine_ShouldHandleMissingPSIData()
    {
        // Arrange
        var combiner = new DataCombiner();
        var gaData = new List<GoogleAnalyticsRecord>
        {
            new() { Date = "2025-10-20", Page = "/home", Users = 120, Sessions = 150, Views = 310 }
        };
        var psiData = new List<PageSpeedInsightsRecord>();

        // Act
        var result = combiner.Combine(gaData, psiData);

        // Assert
        Assert.Single(result);
        Assert.Equal(120, result[0].Users);
        Assert.Null(result[0].PerformanceScore);
        Assert.Null(result[0].LCP_ms);
    }
}

