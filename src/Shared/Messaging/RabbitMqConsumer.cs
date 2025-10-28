using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configuration;

namespace Shared.Messaging;


public class RabbitMqConsumer : IMessageConsumer
{
    private readonly MessagingSettings _settings;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _channelLock = new();
    private bool _disposed;

    public RabbitMqConsumer(
        IOptions<MessagingSettings> settings,
        ILogger<RabbitMqConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }


    public async Task SubscribeAsync<T>(
        string queueName,
        Func<T, Task<bool>> handler,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMqConsumer));

        try
        {
            InitializeConnection();

         
            _channel!.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

          
            _channel.QueueBind(
                queue: queueName,
                exchange: _settings.ExchangeName,
                routingKey: queueName);


            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (deserializedMessage != null)
                    {
                        _logger.LogInformation(
                            "Processing message from queue {QueueName}. MessageType: {MessageType}",
                            queueName,
                            typeof(T).Name);

                    
                        var success = await handler(deserializedMessage);

                        lock (_channelLock)
                        {
                            if (success)
                            {
                                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                                _logger.LogInformation(
                                    "Message processed successfully. Queue: {QueueName}",
                                    queueName);
                            }
                            else
                            {
                            
                                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                                _logger.LogWarning(
                                    "Message processing failed. Message requeued. Queue: {QueueName}",
                                    queueName);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Deserialized message is null for queue {QueueName}",
                            queueName);
                        lock (_channelLock)
                        {
                            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx,
                        "Failed to deserialize message from queue {QueueName}. Message: {Message}",
                        queueName,
                        message);

                    lock (_channelLock)
                    {
                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing message from queue {QueueName}",
                        queueName);

                    lock (_channelLock)
                    {
                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "Started consuming messages from queue: {QueueName}",
                queueName);

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer cancelled for queue: {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RabbitMQ consumer for queue: {QueueName}", queueName);
            throw;
        }
    }

    private void InitializeConnection()
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        if (_settings.AutoCreateInfrastructure)
        {
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);
        }

        _logger.LogInformation("RabbitMQ Consumer connection established. Host: {Host}", _settings.HostName);
    }

    public Task StopAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ Consumer disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Consumer");
        }
    }
}