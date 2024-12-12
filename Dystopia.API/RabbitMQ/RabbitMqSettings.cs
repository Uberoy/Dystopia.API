namespace Dystopia.API.RabbitMQ;

public class RabbitMqSettings
{
    public string Uri { get; set; }
    public string ClientProvidedName { get; set; }
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public string RoutingKey { get; set; }
}