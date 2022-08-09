namespace MessageBus.Abstraction.Interfaces;

public interface IMessageConsumer<T>
{
    void Execute(T @event);
}

public interface IMessageConsumerAsync<T>
{
    Task ExecuteAsync(T @event);
}