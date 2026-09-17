namespace InovaGAB.API.DTOs.Request
{
    public class PrioritizeIdeaRequest
    {
        // "Low", "Medium", "High" ou "Critical"
        public string Priority { get; set; } = "Medium";
    }
}
