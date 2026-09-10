using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InovaGAB.API.Models
{
    public class StrategicGuideline
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public GuidelinePriority Priority { get; set; } =
            GuidelinePriority.Medium;

        public string Category { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedById { get; set; } = string.Empty;

        [BsonIgnore]
        public User CreatedBy { get; set; } = null!;
    }

    public enum GuidelinePriority
    {
        Low,
        Medium,
        High
    }
}
