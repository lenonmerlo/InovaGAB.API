namespace InovaGAB.API.DTOs.Response
{
    public class ChallengeResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Prize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int TotalIdeas { get; set; }
        public List<IdeaResponse> Ideas { get; set; } = new();
    }
}
