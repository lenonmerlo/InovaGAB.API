namespace InovaGAB.API.DTOs.Response
{
    public class AiScoreSuggestionResponse
    {
        public int ImpactScore { get; set; }
        public int FeasibilityScore { get; set; }
        public int AlignmentScore { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
