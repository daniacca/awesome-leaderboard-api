using System.Text;
using System.Text.Json;
using MessageBus.Interfaces;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageBus.RabbitMq.AbsClasses;

public abstract class RabbitProducerBase<T> : RabbitMqBaseClient, IMessageProducer<T>
{
    private ILogger<RabbitProducerBase<T>> Logger { get; }

    protected abstract string? ExchangeName { get; }

    protected abstract string? RoutingKeyName { get; }
    
    protected abstract string AppId { get; }

    protected RabbitProducerBase(
        ConnectionFactory connectionFactory,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> logger,
        ILogger<RabbitProducerBase<T>> producerBaseLogger) :
        base(connectionFactory, settings, logger) => Logger = producerBaseLogger;

    public virtual void Publish(T @event)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            var properties = Channel?.CreateBasicProperties() ?? throw new NullReferenceException(nameof(Channel));
            SetDefaultChannelProperties(properties);
            Channel.BasicPublish(exchange: ExchangeName, routingKey: RoutingKeyName, body: body, basicProperties: properties);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while publishing");
        }
    }

    protected virtual void SetDefaultChannelProperties(IBasicProperties properties)
    {
        properties.AppId = AppId;
        properties.ContentType = "application/json";
        properties.DeliveryMode = 1;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }
}