using AnalyticsAggregator.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AnalyticsAggregator.Infrastructure.Services;

public class DataIngestionService : IDataIngestionService
{
    private readonly IGoogleAnalyticsAdapter _gaAdapter;
    private readonly IPageSpeedInsightsAdapter _psiAdapter;
    private readonly IDataCombiner _dataCombiner;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<DataIngestionService> _logger;
    private readonly string _gaDataPath;
    private readonly string _psiDataPath;

    public DataIngestionService(
        IGoogleAnalyticsAdapter gaAdapter,
        IPageSpeedInsightsAdapter psiAdapter,
        IDataCombiner dataCombiner,
        IRabbitMQService rabbitMQService,
        ILogger<DataIngestionService> logger,
        IConfiguration configuration)
    {
        _gaAdapter = gaAdapter;
        _psiAdapter = psiAdapter;
        _dataCombiner = dataCombiner;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _gaDataPath = configuration["DataPaths:GA"] ?? "Data/MockData/ga_data.json";
        _psiDataPath = configuration["DataPaths:PSI"] ?? "Data/MockData/psi_data.json";
    }

    public async Task IngestDataAsync()
    {
        _logger.LogInformation("Starting data ingestion...");

        var gaData = await _gaAdapter.ReadDataAsync(_gaDataPath);
        _logger.LogInformation("Read {Count} GA records", gaData.Count);

        var psiData = await _psiAdapter.ReadDataAsync(_psiDataPath);
        _logger.LogInformation("Read {Count} PSI records", psiData.Count);

        var combined = _dataCombiner.Combine(gaData, psiData);
        _logger.LogInformation("Combined into {Count} records", combined.Count);

        foreach (var record in combined)
        {
            var json = JsonSerializer.Serialize(record);
            _rabbitMQService.Publish("analytics.raw", "analytics.raw", json);
            _logger.LogInformation("Published record: {Page} - {Date}", record.Page, record.Date);
        }

        _logger.LogInformation("Data ingestion completed. Published {Count} messages", combined.Count);
    }
}

