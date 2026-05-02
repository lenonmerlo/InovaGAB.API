namespace InovaGAB.API.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public IdeaStatus Status { get; set; } = IdeaStatus.Submitted;
        public int ImpactScore { get; set; } = 0;
        public int FeasibilityScore { get; set; } = 0;
        public int AlignmentScore { get; set; } = 0;
        public int TotalScore => (ImpactScore + FeasibilityScore + AlignmentScore) / 3;
        public string? EvidenceUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ChallengId { get; set; }
        public Challenge? Challenge { get; set; }
    }

    public enum  IdeaStatus
    {
        Submitted,
        UnderReview,
        Approved,
        Rejected
    }
}
