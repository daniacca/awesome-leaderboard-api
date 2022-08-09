using Events.Messages;
using MessageBus.RabbitMq.AbsClasses;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AwesomeLeaderboard.Services;

public class UpdateUserMessageProducer : RabbitProducerBase<UpdateUserScore>
{
	public UpdateUserMessageProducer(
		ConnectionFactory connectionFactory,
		IOptions<RabbitClientConfiguration> settings,
		ILogger<RabbitMqBaseClient> logger,
		ILogger<RabbitProducerBase<UpdateUserScore>> producerBaseLogger) :
		base(connectionFactory, settings, logger, producerBaseLogger)
	{
	}

	protected override string AppId => nameof(UpdateUserMessageProducer);

    protected override string? ExchangeName => "USER_HOST.UserExchange";

    protected override string? RoutingKeyName => "user.update";
}