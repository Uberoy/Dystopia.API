using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;

namespace Dystopia.API.RabbitMQ
{
    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IOptions<RabbitMqSettings> rabbitMqSettings, ILogger<RabbitMqService> logger)
        {
            _logger = logger;

            var settings = rabbitMqSettings.Value;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(settings.Uri),
                ClientProvidedName = settings.ClientProvidedName
            };

            const int maxRetries = 10;
            const int delayInSeconds = 10;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("Attempting to connect to RabbitMQ. Attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _exchangeName = settings.ExchangeName;
                    _queueName = settings.QueueName;
                    _routingKey = settings.RoutingKey;

                    _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
                    _channel.QueueDeclare(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    _channel.QueueBind(_queueName, _exchangeName, _routingKey, arguments: null);

                    _logger.LogInformation("Connected to RabbitMQ successfully.");
                    break; 
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ connection failed. Attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

                    if (attempt == maxRetries)
                    {
                        _logger.LogError("Max retry attempts reached. Unable to connect to RabbitMQ.");
                        throw; 
                    }

                    _logger.LogInformation("Retrying connection in {DelayInSeconds} seconds...", delayInSeconds);
                    Thread.Sleep(delayInSeconds * 1000); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while connecting to RabbitMQ.");
                    throw; 
                }
            }
        }

        public void PublishMessage(string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(_exchangeName, _routingKey, null, body);
                _logger.LogInformation("Message published successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ.");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing RabbitMQ connection.");
            }
        }
    }
}
