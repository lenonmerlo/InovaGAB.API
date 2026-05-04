namespace InovaGAB.API.DTOs.Request
{
    public class CreateIdeaRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string? EvidenceUrl { get; set; }
        public int? ChallengeId { get; set; }
    }
}
