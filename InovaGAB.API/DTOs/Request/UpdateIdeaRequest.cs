namespace InovaGAB.API.DTOs.Request
{
    // Não expõe autor, scores, status ou prioridade: o Operator não
    // altera esses campos por este endpoint.
    public class UpdateIdeaRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Division { get; set; }
        public string? EvidenceUrl { get; set; }
        public string? ChallengeId { get; set; }
        public string? GuidelineId { get; set; }
    }
}
