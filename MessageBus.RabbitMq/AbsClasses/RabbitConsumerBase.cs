using System.Text;
using System.Text.Json;
using MessageBus.Abstraction.Interfaces;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.RabbitMq.AbsClasses;

public abstract class RabbitConsumerBase<TBody> : RabbitMqBaseClient, IMessageConsumerAsync<TBody>
{
    ILogger<RabbitConsumerBase<TBody>> Logger { get; }

    protected abstract string? QueueName { get; }

    protected RabbitConsumerBase(ConnectionFactory connectionFactory,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> loggerBase,
        ILogger<RabbitConsumerBase<TBody>> logger)
        : base(connectionFactory, settings, loggerBase)
    {
        Logger = logger;
        try
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += OnEventReceived;
            Channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while consuming message");
        }
    }

    protected virtual async Task OnEventReceived(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            var body = Encoding.UTF8.GetString(@event.Body.ToArray());
            var message = JsonSerializer.Deserialize<TBody>(body);
            await ExecuteAsync(message ?? throw new NullReferenceException(nameof(message)));
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while retrieving message from queue.");
            Console.WriteLine("OnEventReceivedError");
        }
        finally
        {
            Channel?.BasicAck(@event.DeliveryTag, false);
        }
    }

    public abstract Task ExecuteAsync(TBody @event);
}
