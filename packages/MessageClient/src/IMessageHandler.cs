namespace MessageClient;

public interface IMessageHandler<in T> where T : class
{
    Task Handle(T message, CancellationToken cancellationToken);
}
