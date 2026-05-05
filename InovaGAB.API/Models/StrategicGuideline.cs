namespace InovaGAB.API.Models
{
    public class StrategicGuideline
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GuidelinePriority Priority { get; set; } = GuidelinePriority.Medium;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
    }

    public enum GuidelinePriority
    {
        Low,
        Medium,
        High
    }
}
