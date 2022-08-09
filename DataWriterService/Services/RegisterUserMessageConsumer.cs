using Common.DistribuitedCache.Interfaces;
using Common.DistribuitedCache.Manager.Commands;
using DataAccess.Interfaces;
using Events.Messages;
using Events.Types;
using MessageBus.RabbitMq.AbsClasses;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Options;
using Models.Db;
using RabbitMQ.Client;

namespace DataWriterService.Services;

public class RegisterUserMessageConsumer : RabbitConsumerBase<RegisterUser>, IHostedService
{
    IUserRepository UserRepository { get; }

    IDistributedCacheManager CacheManager { get; }

    public RegisterUserMessageConsumer(
        ConnectionFactory connectionFactory,
        IUserRepository repository,
        IDistributedCacheManager cacheManager,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> loggerBase,
        ILogger<RabbitConsumerBase<RegisterUser>> loggerConsumerBase) :
        base(connectionFactory, settings, loggerBase, loggerConsumerBase)
    {
        UserRepository = repository;
        CacheManager = cacheManager;
    }

    protected override string? QueueName => "USER_HOST.user.register";

    protected async Task SaveUpdateOnCache(RegisterUser @event)
    {
        var setCommand = new SetCommand<RegisterUser>(new SetCommandPayload<RegisterUser>
        {
            Key = @event.Id.ToString(),
            Data = @event
        });

        await CacheManager.ExecuteAsync(setCommand);
    }

    public async override Task ExecuteAsync(RegisterUser @event)
    {
        await Task.Run(() => Console.WriteLine($"Register message received, content => {{ id: {@event.Id}, username: {@event.Username} }}"));

        @event.State = EventState.InProgress;
        await SaveUpdateOnCache(@event);

        bool saveResult = false;
        if(@event.Username is not null)
        {
            try
            {
                saveResult = await UserRepository.AddAsync(new User
                {
                    Username = @event.Username,
                    Score = @event.InitialScore
                });
            }
            catch
            {
                @event.State = EventState.FinishWithError;
                await SaveUpdateOnCache(@event);
                return;
            }
        }

        @event.State = saveResult ? EventState.Done : EventState.FinishWithError;
        await SaveUpdateOnCache(@event);
    }

    public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}