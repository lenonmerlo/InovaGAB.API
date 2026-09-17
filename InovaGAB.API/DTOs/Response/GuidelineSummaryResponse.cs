namespace InovaGAB.API.DTOs.Response
{
    public class GuidelineSummaryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Campaign { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
