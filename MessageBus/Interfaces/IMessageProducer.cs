namespace MessageBus.Interfaces;

public interface IMessageProducer<in T>
{
    void Publish(T message);
}

public interface IMessageProducerAsync<in T>
{
    Task PublishAsync(T message);
}