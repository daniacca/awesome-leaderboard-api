using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageBus.RabbitMq.AbsClasses;

public class RabbitMqBaseClient : IDisposable
{
    protected RabbitClientConfiguration ClientConfiguration { get; }

    protected IModel? Channel { get; private set; }
    private IConnection? Connection { get; set; }

    private ConnectionFactory ConnectionFactory { get; }
    private ILogger<RabbitMqBaseClient> Logger { get; }

    protected RabbitMqBaseClient(
        ConnectionFactory connectionFactory,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> logger)
    {
        ClientConfiguration = settings.Value;
        ConnectionFactory = connectionFactory;
        Logger = logger;
        ConnectToRabbitMq();
    }

    protected virtual void ConnectToRabbitMq()
    {
        if (Connection is null || !Connection.IsOpen)
            Connection = ConnectionFactory.CreateConnection();
        
        if (Channel is null || !Channel.IsOpen)
        {
            Channel = Connection.CreateModel();

            foreach(var exchange in ClientConfiguration.Exchanges)
                Channel.ExchangeDeclare(exchange.Name, type: exchange.Type, durable: true, autoDelete: false);

            foreach(var queue in ClientConfiguration.Queues)
                Channel.QueueDeclare(queue.Name, durable: false, exclusive: false, autoDelete: false);

            foreach(var bind in ClientConfiguration.QueueBindings)
                Channel.QueueBind(bind.Queue, bind.Exchange, bind.RoutingKey);
        }
    }

    public void Dispose()
    {
        try
        {
            Channel?.Close();
            Channel?.Dispose();

            Connection?.Close();
            Connection?.Dispose();

            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
        }
    }
}