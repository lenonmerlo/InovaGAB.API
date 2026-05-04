namespace InovaGAB.API.DTOs.Response
{
    public class IdeaResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ImpactScore { get; set; }
        public int FeasibilityScore { get; set; }
        public int AlignmentScore { get; set; }
        public int TotalScore { get; set; }
        public string? EvidenceUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;

    }
}
