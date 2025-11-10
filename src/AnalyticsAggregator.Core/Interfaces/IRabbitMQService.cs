namespace AnalyticsAggregator.Core.Interfaces;

public interface IRabbitMQService
{
    void Publish(string exchange, string routingKey, string message);
    void Dispose();
}

