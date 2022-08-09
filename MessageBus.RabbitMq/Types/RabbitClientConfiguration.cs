using RabbitMQ.Client;

namespace MessageBus.RabbitMq.Types;

public class ExchangeDeclaring
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Exchange type, for available type
    /// see <see cref="ExchangeType"/>
    /// </summary>
    public string? Type { get; set; }
}

public class QueueDeclaring
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string? Name { get; set; }
}

public class QueueBindings
{
    /// <summary>
    /// Set this to the Queue Name you want to bind
    /// </summary>
    public string? Queue { get; set; }

    /// <summary>
    /// Set this to the Exchange Name you want to bind
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Set this to the Routing Key you want to use to the binding
    /// </summary>
    public string? RoutingKey { get; set; }
}

public class RabbitClientConfiguration
{
    public string? User { get; set; }

    public string? Password { get; set; }

    public string? HostName { get; set; }

    public int Port { get; set; }

    public string? VirtualHost { get; set; }

    public List<ExchangeDeclaring> Exchanges { get; set; } = new List<ExchangeDeclaring>();

    public List<QueueDeclaring> Queues { get; set; } = new List<QueueDeclaring>();

    public List<QueueBindings> QueueBindings { get; set; } = new List<QueueBindings>();
}