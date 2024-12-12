namespace Dystopia.API.Entities;

public class Sender
{
    public string ExchangeName { get; set; }
    public string RoutingKey { get; set; }
    public string QueueName { get; set; }
}