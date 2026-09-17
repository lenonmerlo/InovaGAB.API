namespace InovaGAB.API.DTOs.Response
{
    public class GuidelineResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Campaign { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsCurrent { get; set; }
        public int Version { get; set; }
        public string RootId { get; set; } = string.Empty;
        public string? PreviousVersionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }
}
