namespace InovaGAB.API.DTOs.Request
{
    public class CreateProjectRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Division {  get; set; } = string.Empty;

        public decimal Investment { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime Deadline {  get; set; }

        public int? IdeaId {  get; set; }
    }
}
