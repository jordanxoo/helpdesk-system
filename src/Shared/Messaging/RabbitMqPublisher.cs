using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Configuration;

namespace Shared.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly MessagingSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqPublisher(
        IOptions<MessagingSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _logger.LogInformation("RabbitMqPublisher created. Will connect on first publish.");
    }


    private void InitializeRabbitMq()
    {
        try
        {
            _logger.LogInformation("Initializing RabbitMQ connection to {Host}:{Port}...", 
                _settings.HostName, _settings.Port);
            
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

            _logger.LogInformation("Creating RabbitMQ connection...");
            _connection = factory.CreateConnection();
            _logger.LogInformation("Creating RabbitMQ channel...");
            _channel = _connection.CreateModel();

            if (_settings.AutoCreateInfrastructure)
            {
                _logger.LogInformation("Declaring exchange {Exchange}...", _settings.ExchangeName);
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);
                
                _logger.LogInformation(
                    "RabbitMQ Publisher initialized successfully. Exchange: {Exchange}, Host: {Host}",
                    _settings.ExchangeName,
                    _settings.HostName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }
    public async Task PublishAsync<T>(T message, string routingKey) where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMqPublisher));

        try
        {
            lock (_lock)
            {
                EnsureConnection();

                var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var body = Encoding.UTF8.GetBytes(jsonMessage);

                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = _settings.PersistentMessages;
                properties.ContentType = "application/json";
                properties.DeliveryMode = _settings.PersistentMessages ? (byte)2 : (byte)1;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "Published message to RabbitMQ. RoutingKey: {RoutingKey}, MessageType: {MessageType}",
                routingKey,
                typeof(T).Name);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish message to RabbitMQ. RoutingKey: {RoutingKey}, MessageType: {MessageType}",
                routingKey,
                typeof(T).Name);
            throw;
        }
    }

    private void EnsureConnection()
    {
        if (_channel == null || _channel.IsClosed || _connection == null || !_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ connection lost. Reconnecting...");
            InitializeRabbitMq();
        }
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

            _logger.LogInformation("RabbitMQ Publisher disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Publisher");
        }
    }
}