using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InovaGAB.API.Models;

public class Challenge
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Prize { get; set; } = 0;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }

    [BsonIgnore]
    public User CreatedBy { get; set; } = null!;

    [BsonIgnore]
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}
