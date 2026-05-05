using InovaGAB.API.Models;

namespace InovaGAB.API.DTOs.Request
{
    public class UpdateProjectRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ProjectStatus? Status {  get; set; }
        public ProjectStage? Stage { get; set; }
        public decimal? Investment { get; set; }
        public decimal? FinancialReturn {  get; set; }
        public int? ProductivityGain { get; set; }
        public int? ProgressPercent {  get; set; }
        public DateTime? Deadline { get; set;  }
    }
}
