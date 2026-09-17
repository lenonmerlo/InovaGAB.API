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

        public string Campaign { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // true somente na versão vigente da linha de histórico (RootId)
        public bool IsCurrent { get; set; } = true;

        public int Version { get; set; } = 1;

        // id da primeira versão da linha; igual ao próprio Id na criação
        [BsonRepresentation(BsonType.ObjectId)]
        public string RootId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? PreviousVersionId { get; set; }

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
