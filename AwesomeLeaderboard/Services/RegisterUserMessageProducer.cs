using Events.Messages;
using MessageBus.RabbitMq.AbsClasses;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AwesomeLeaderboard.Services;

public class RegisterUserMessageProducer : RabbitProducerBase<RegisterUser>
{
	public RegisterUserMessageProducer(
        ConnectionFactory connectionFactory,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> logger,
        ILogger<RabbitProducerBase<RegisterUser>> producerBaseLogger) :
        base(connectionFactory, settings, logger, producerBaseLogger)
	{
	}

    protected override string AppId => nameof(RegisterUserMessageProducer);

    protected override string? ExchangeName => "USER_HOST.UserExchange";

    protected override string? RoutingKeyName => "user.register";
}