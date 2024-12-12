using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dystopia.API.Entities;

public class Ticket
{
    [BsonRepresentation(BsonType.ObjectId), BsonId]
    public string? Id { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public DateTime DateCreated { get; set; }
}