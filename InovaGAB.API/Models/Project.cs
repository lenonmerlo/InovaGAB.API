namespace InovaGAB.API.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public ProjectStage Stage { get; set; } = ProjectStage.Diagnosis;
        public decimal Investment { get; set; } = 0;
        public decimal FinancialReturn { get; set; } = 0;
        public decimal Roi => Investment > 0 ? (FinancialReturn - Investment) / Investment * 100 : 0;
        public int ProductivityGain { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        public int ProgressPercent { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set;} 

        public int ManagerId { get; set; }
        public User Manager { get; set; } = null!;

        public int? IdeaId { get; set; }
        public Idea? Idea { get; set; }

    }

    public enum ProjectStatus
    {
        Planning,
        InProgress,
        Completed,
        OnHold,
        Cancelled
    }

    public enum ProjectStage
    {
        Diagnosis,
        Implementation,
        Validation,
        Closure
    }
}
