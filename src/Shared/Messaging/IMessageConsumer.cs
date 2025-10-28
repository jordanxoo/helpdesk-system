namespace Shared.Messaging;


public interface IMessageConsumer : IDisposable
{
   
    /// Subskrybuj do kolejki i przetwarzaj wiadomości
  
    /// <typeparam name="T">Typ wiadomości (musi być klasą)</typeparam>
    /// <param name="queueName">Nazwa kolejki RabbitMQ</param>
    /// <param name="handler">Callback do przetwarzania wiadomości</param>
    /// <param name="cancellationToken">Token anulowania</param>
    Task SubscribeAsync<T>(
        string queueName,
        Func<T, Task<bool>> handler,
        CancellationToken cancellationToken = default) where T : class;

    Task StopAsync();
}