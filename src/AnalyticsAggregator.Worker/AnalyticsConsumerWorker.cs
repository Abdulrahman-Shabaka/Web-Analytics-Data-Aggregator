using AnalyticsAggregator.Core.Entities;
using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Infrastructure.Data;
using AnalyticsAggregator.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnalyticsAggregator.Worker;

public class AnalyticsConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsConsumerWorker> _logger;
    private readonly string _rabbitMQHostName;
    private IConnection? _connection;
    private IModel? _channel;

    public AnalyticsConsumerWorker(
        IServiceProvider serviceProvider,
        ILogger<AnalyticsConsumerWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQHostName = configuration["RabbitMQ:HostName"] ?? "localhost";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var deliveryTag = ea.DeliveryTag;

            _logger.LogInformation("Received message: {Message}", message);

            var success = await ProcessMessageAsync(message, deliveryTag);
            
            if (success)
            {
                _channel?.BasicAck(deliveryTag, false);
                _logger.LogInformation("Message processed and acknowledged");
            }
            else
            {
                _channel?.BasicNack(deliveryTag, false, true);
                _logger.LogWarning("Message processing failed, requeued");
            }
        };

        _channel?.BasicConsume(queue: "analytics.raw.q", autoAck: false, consumer: consumer);

        _logger.LogInformation("Consumer started, waiting for messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private Task SendToDLQAsync(string message, string reason, ulong deliveryTag)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var rabbitMQService = scope.ServiceProvider.GetRequiredService<IRabbitMQService>();
            
            var dlqMessage = new
            {
                originalMessage = message,
                reason = reason,
                failedAt = DateTime.UtcNow,
                deliveryTag = deliveryTag
            };
            
            var dlqJson = System.Text.Json.JsonSerializer.Serialize(dlqMessage);
            rabbitMQService.Publish("analytics.dlq", "analytics.dlq", dlqJson);
            
            _logger.LogWarning("Message sent to DLQ. Reason: {Reason}", reason);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to DLQ");
            return Task.CompletedTask;
        }
    }

    private async Task<bool> ProcessMessageAsync(string message, ulong deliveryTag)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var aggregationService = scope.ServiceProvider.GetRequiredService<IAggregationService>();

                var record = JsonSerializer.Deserialize<CombinedAnalyticsRecord>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (record == null)
                {
                    _logger.LogError("Failed to deserialize message");
                    return false;
                }

                if (!DateTime.TryParse(record.Date, out var date))
                {
                    _logger.LogError("Invalid date format: {Date}", record.Date);
                    return false;
                }

                // Ensure date is UTC for PostgreSQL
                if (date.Kind == DateTimeKind.Unspecified)
                {
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                }
                else if (date.Kind == DateTimeKind.Local)
                {
                    date = date.ToUniversalTime();
                }

                var rawData = new RawData
                {
                    Date = date,
                    Page = record.Page,
                    Users = record.Users,
                    Sessions = record.Sessions,
                    Views = record.Views,
                    PerformanceScore = record.PerformanceScore,
                    LCPms = record.LCP_ms,
                    ReceivedAt = DateTime.UtcNow
                };

                context.RawData.Add(rawData);
                await context.SaveChangesAsync();

                _logger.LogInformation("Saved RawData: {Page} - {Date}", rawData.Page, rawData.Date);

                await aggregationService.AggregateDailyStatsAsync(rawData);

                _logger.LogInformation("Aggregated daily stats for {Date}", date);
                return true;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, "Error processing message (attempt {RetryCount}/{MaxRetries})", retryCount, maxRetries);

                if (retryCount < maxRetries)
                {
                    var delay = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff
                    await Task.Delay(delay);
                }
                else
                {
                    // Send to DLQ after max retries
                    await SendToDLQAsync(message, ex.Message, deliveryTag);
                    return true; // Acknowledge original message since we sent to DLQ
                }
            }
        }

        return false;
    }

    private async Task ConnectAsync()
    {
        var factory = new ConnectionFactory { HostName = _rabbitMQHostName };
        
        for (int i = 0; i < 10; i++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare("analytics.raw.q", durable: true, exclusive: false, autoDelete: false);
                _logger.LogInformation("Connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/10)", i + 1);
                await Task.Delay(2000);
            }
        }

        throw new Exception("Failed to connect to RabbitMQ after 10 attempts");
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

internal record CombinedAnalyticsRecord
{
    public string Page { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Views { get; set; }
    public decimal? PerformanceScore { get; set; }
    public int? LCP_ms { get; set; }
}

