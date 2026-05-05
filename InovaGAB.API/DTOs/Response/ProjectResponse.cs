namespace InovaGAB.API.DTOs.Response
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division {  get; set; } = string.Empty;
        public string Status {  get; set; } = string.Empty;
        public string Stage {  get; set; } = string.Empty;
        public decimal Investment { get; set; }
        public decimal FinancialReturn { get; set; }
        public decimal Roi {  get; set; }
        public int ProductivityGain { get; set; }
        public DateTime StartDate { get; set;  }
        public DateTime Deadline {  get; set; }
        public int ProgressPercent { get; set; }
        public DateTime CreatedAt {  get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public int? IdeaId { get; set;  }
    }
}
