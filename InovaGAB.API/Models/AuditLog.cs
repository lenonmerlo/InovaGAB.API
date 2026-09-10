using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InovaGAB.API.Models;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } =
        ObjectId.GenerateNewId().ToString();

    public string Method { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public string? UserRole { get; set; }

    public int StatusCode { get; set; }

    public long DurationMs { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
}