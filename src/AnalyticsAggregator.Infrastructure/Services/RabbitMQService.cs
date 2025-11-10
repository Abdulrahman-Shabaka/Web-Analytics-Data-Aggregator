using AnalyticsAggregator.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace AnalyticsAggregator.Infrastructure.Services;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(string hostName, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange and queue
        _channel.ExchangeDeclare("analytics.raw", ExchangeType.Direct, durable: true);
        
        // Declare DLQ
        _channel.ExchangeDeclare("analytics.dlq", ExchangeType.Direct, durable: true);
        _channel.QueueDeclare("analytics.dlq", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("analytics.dlq", "analytics.dlq", "analytics.dlq");
        
        // Declare main queue with DLQ arguments
        var queueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "analytics.dlq" },
            { "x-dead-letter-routing-key", "analytics.dlq" }
        };
        _channel.QueueDeclare("analytics.raw.q", durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
        _channel.QueueBind("analytics.raw.q", "analytics.raw", "analytics.raw");

        _logger.LogInformation("RabbitMQ connection established");
    }

    public void Publish(string exchange, string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        _logger.LogInformation("Published message to {Exchange} with routing key {RoutingKey}", exchange, routingKey);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

