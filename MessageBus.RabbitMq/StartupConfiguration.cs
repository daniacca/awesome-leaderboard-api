using MessageBus.RabbitMq.AbsClasses;
using MessageBus.RabbitMq.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace MessageBus.RabbitMq;

public static class StartupConfiguration
{
    /// <summary>
    /// Add binding for RabbitConfiguration on <paramref name="services"/>,
    /// get connection data from <paramref name="configuration"/> and create a
    /// singleton service for ConnectionFactory
    /// </summary>
    /// <param name="services">ServiceCollection</param>
    /// <param name="configuration">IConfiguration</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection ConfigureRabbitClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<RabbitClientConfiguration>(option => configuration.GetSection(nameof(RabbitClientConfiguration)).Bind(option))
            .AddSingleton(serviceProvider =>
            {
                var rabbitConfiguration = configuration.GetSection(nameof(RabbitClientConfiguration)).Get<RabbitClientConfiguration>();
                if (rabbitConfiguration is null)
                    throw new Exception();

                var decompose = (RabbitClientConfiguration conf) => (conf.User, conf.Password, conf.HostName, conf.Port, conf.VirtualHost);
                var (User, Password, HostName, Port, VirtualHost) = decompose(rabbitConfiguration);
                return new ConnectionFactory
                {
                    Uri = new Uri($"amqp://{User}:{Password}@{HostName}:{Port}/{VirtualHost}"),
                    DispatchConsumersAsync = true,
                };
            });
            
        return services;
    }

    /// <summary>
    /// Add a Message Producer Service on <paramref name="services"/> with the
    /// specified <paramref name="lifetime"/>.
    /// </summary>
    /// <typeparam name="TMessage">the type of the message to be sent</typeparam>
    /// <typeparam name="TAbstract">the abstract type which you want to register the TProducer on the service collection</typeparam>
    /// <typeparam name="TProducer">the concrete type which you want to be resolved when asking for <typeparamref name="TAbstract"/></typeparam>
    /// <param name="services">the service collection</param>
    /// <param name="lifetime">the service lifetime</param>
    /// <returns>the service collection with the added service</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IServiceCollection AddRabbitProducer<TMessage, TAbstract, TProducer>(this IServiceCollection services, ServiceLifetime lifetime)
        where TAbstract : RabbitProducerBase<TMessage>
        where TProducer : TAbstract
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient<TAbstract, TProducer>(),
            ServiceLifetime.Scoped => services.AddScoped<TAbstract, TProducer>(),
            ServiceLifetime.Singleton => services.AddSingleton<TAbstract, TProducer>(),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Register a concrete <typeparamref name="TConsumer"/> type as Hosted Service
    /// for managing the receiving of messages from a specified queue from rabbitmq
    /// </summary>
    /// <typeparam name="TBody"></typeparam>
    /// <typeparam name="TConsumer"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRabbitMessageConsumer<TBody, TConsumer>(this IServiceCollection services)
        where TBody: class
        where TConsumer: RabbitConsumerBase<TBody>, IHostedService
    {
        return services.AddHostedService<TConsumer>();
    }
}