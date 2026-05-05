namespace InovaGAB.API.DTOs.Request
{
    public class CreateGuidelineRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";


    }
}
