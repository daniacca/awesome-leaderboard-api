using Common.DistribuitedCache.Interfaces;
using Common.DistribuitedCache.Manager.Commands;
using DataAccess.Interfaces;
using Events.Messages;
using Events.Types;
using MessageBus.RabbitMq.AbsClasses;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Linq;

namespace DataWriterService.Services;

public class UpdateUserMessageConsumer : RabbitConsumerBase<UpdateUserScore>
{
    IUserRepository UserRepository { get; }

    IDistributedCacheManager CacheManager { get; }

    ILogger<UpdateUserMessageConsumer> Logger { get; }

    public UpdateUserMessageConsumer(
        ConnectionFactory connectionFactory,
        IUserRepository repository,
        IDistributedCacheManager cacheManager,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> loggerBase,
        ILogger<RabbitConsumerBase<UpdateUserScore>> loggerConsumerBase,
        ILogger<UpdateUserMessageConsumer> logger) :
        base(connectionFactory, settings, loggerBase, loggerConsumerBase)
    {
        UserRepository = repository;
        CacheManager = cacheManager;
        Logger = logger;
    }

    protected override string? QueueName => "USER_HOST.user.update";

    protected async Task UpdateEventStateOnCache(UpdateUserScore @event, EventState newState)
    {
        @event.State = newState;
        var setCommand = new SetCommand<UpdateUserScore>(new SetCommandPayload<UpdateUserScore>
        {
            Key = @event.Id.ToString(),
            Data = @event
        });

        await CacheManager.ExecuteAsync(setCommand);
    }

    public async override Task ExecuteAsync(UpdateUserScore @event)
    {
        await Task.Run(() => Console.WriteLine($"Update message received, content => {{ id: {@event.Id}, username: {@event.Username}, newScore: {@event.NewScore} }}"));
        await UpdateEventStateOnCache(@event, EventState.InProgress);

        var result = false;
        if (@event.Username is not null)
        {
            try
            {
                var userFromDB = (await UserRepository.GetListAsync(u => u.Username == @event.Username)).FirstOrDefault();
                if (userFromDB is not null)
                {
                    userFromDB.Score = @event.NewScore;
                    result = await UserRepository.UpdateAsync(userFromDB);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Error while updating DB data");
            }
        }

        await UpdateEventStateOnCache(@event, result ? EventState.Done : EventState.FinishWithError);
    }
}