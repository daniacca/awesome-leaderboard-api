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

public class RegisterUserMessageConsumer : RabbitConsumerBase<RegisterUser>
{
    IUserRepository UserRepository { get; }

    IDistributedCacheManager CacheManager { get; }

    ILogger<RegisterUserMessageConsumer> Logger { get; }

    public RegisterUserMessageConsumer(
        ConnectionFactory connectionFactory,
        IUserRepository repository,
        IDistributedCacheManager cacheManager,
        IOptions<RabbitClientConfiguration> settings,
        ILogger<RabbitMqBaseClient> loggerBase,
        ILogger<RabbitConsumerBase<RegisterUser>> loggerConsumerBase,
        ILogger<RegisterUserMessageConsumer> logger) :
        base(connectionFactory, settings, loggerBase, loggerConsumerBase)
    {
        UserRepository = repository;
        CacheManager = cacheManager;
        Logger = logger;
    }

    protected override string? QueueName => "USER_HOST.user.register";

    protected async Task UpdateEventStateOnCache(RegisterUser @event, EventState newState)
    {
        @event.State = newState;
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
        await UpdateEventStateOnCache(@event, EventState.InProgress);
         
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
            catch(Exception ex)
            {
                Logger.LogError(ex, "Error when saving data on DB");
            }
        }

        await UpdateEventStateOnCache(@event, saveResult ? EventState.Done : EventState.FinishWithError);
    }
}