using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InovaGAB.API.Models
{
    public class Idea
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Division { get; set; } = string.Empty;

        public IdeaStatus Status { get; set; } = IdeaStatus.Submitted;

        // prioridade de triagem definida pelo Manager, sem relação com GuidelinePriority
        public IdeaPriority Priority { get; set; } = IdeaPriority.Medium;

        // exclusão lógica (ver decisão no README)
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public int ImpactScore { get; set; } = 0;

        public int FeasibilityScore { get; set; } = 0;

        public int AlignmentScore { get; set; } = 0;

        [BsonIgnore]
        public int TotalScore =>
            (ImpactScore + FeasibilityScore + AlignmentScore) / 3;

        public string? EvidenceUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonIgnore]
        public User User { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? ChallengeId { get; set; }

        [BsonIgnore]
        public Challenge? Challenge { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? GuidelineId { get; set; }

        [BsonIgnore]
        public StrategicGuideline? Guideline { get; set; }
    }

    public enum IdeaStatus
    {
        Submitted,
        UnderReview,
        Approved,
        Rejected
    }

    public enum IdeaPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
