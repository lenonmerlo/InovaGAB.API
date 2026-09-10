using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InovaGAB.API.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Division { get; set; } = string.Empty;

        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        public ProjectStage Stage { get; set; } = ProjectStage.Diagnosis;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Investment { get; set; } = 0;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal FinancialReturn { get; set; } = 0;

        [BsonIgnore]
        public decimal Roi =>
            Investment > 0
                ? (FinancialReturn - Investment) / Investment * 100
                : 0;

        public int ProductivityGain { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime Deadline { get; set; }

        public int ProgressPercent { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ManagerId { get; set; } = string.Empty;

        [BsonIgnore]
        public User Manager { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? IdeaId { get; set; }

        [BsonIgnore]
        public Idea? Idea { get; set; }
    }

    public enum ProjectStatus
    {
        Planning,
        InProgress,
        Completed,
        OnHold,
        Cancelled
    }

    public enum ProjectStage
    {
        Diagnosis,
        Implementation,
        Validation,
        Closure
    }
}
