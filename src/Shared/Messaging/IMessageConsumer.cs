namespace Shared.Messaging;

public interface IMessageConsumer : IDisposable
{
    /// <summary>
    /// Subscribes to a queue and processes messages.
    /// </summary>
    /// <typeparam name="T">Message type (must be a class)</typeparam>
    /// <param name="queueName">RabbitMQ queue name</param>
    /// <param name="handler">Callback to process messages. Returns true for ACK, false for NACK with requeue.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SubscribeAsync<T>(
        string queueName,
        Func<T, Task<bool>> handler,
        CancellationToken cancellationToken = default) where T : class;

    Task StopAsync();
}