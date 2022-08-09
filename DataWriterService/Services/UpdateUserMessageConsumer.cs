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

public class UpdateUserMessageConsumer : RabbitConsumerBase<UpdateUserScore>, IHostedService
{
    IUserRepository UserRepository { get; }

    IDistributedCacheManager CacheManager { get; }

    public UpdateUserMessageConsumer(
        ConnectionFactory connectionFactory,
        IUserRepository repository,
        IDistributedCacheManager cacheManager,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> loggerBase,
        ILogger<RabbitConsumerBase<UpdateUserScore>> loggerConsumerBase) :
        base(connectionFactory, settings, loggerBase, loggerConsumerBase)
    {
        UserRepository = repository;
        CacheManager = cacheManager;
    }

    protected override string? QueueName => "USER_HOST.user.update";

    protected async Task SaveUpdateOnCache(UpdateUserScore @event)
    {
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

        @event.State = EventState.InProgress;
        await SaveUpdateOnCache(@event);

        var result = false;
        if (@event.Username is not null)
        {
            try
            {
                var userFromDB = (await UserRepository.GetListAsync(u => u.Username == @event.Username)).FirstOrDefault();
                if (userFromDB is null)
                {
                    @event.State = EventState.FinishWithError;
                    await SaveUpdateOnCache(@event);
                    return;
                }

                userFromDB.Score = @event.NewScore;
                result = await UserRepository.UpdateAsync(userFromDB);
            }
            catch
            {
                @event.State = EventState.FinishWithError;
                await SaveUpdateOnCache(@event);
                return;
            }
        }

        @event.State = result ? EventState.Done : EventState.FinishWithError;
        await SaveUpdateOnCache(@event);
    }

    public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}