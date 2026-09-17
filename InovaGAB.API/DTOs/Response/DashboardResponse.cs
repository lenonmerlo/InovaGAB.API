namespace InovaGAB.API.DTOs.Response
{
    public class DashboardResponse
    {
        public decimal TotalRoi { get; set; }
        public decimal TotalSavings { get; set; }
        public int ProductivityGainAverage { get; set; }
        public int ActiveProjects { get; set; }
        public int DelayedProjects { get; set; }
        public IdeaFunnelDto IdeaFunnel { get; set; } = new();
        public List<ProjectResponse> TopProjects { get; set; } = new();
        public List<RankingItemDto> TopContributors { get; set; } = new();
        public List<GuidelineDashboardDto> GuidelineBreakdown { get; set; } = new();
    }

    public class IdeaFunnelDto
    {
        public int TotalSubmitted { get; set; }
        public int UnderReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int ConvertedToProjects { get; set; }
    }

    public class RankingItemDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public int Points { get; set; }
        public int IdeasApproved { get; set; }
    }

    public class GuidelineDashboardDto
    {
        // null quando o grupo representa projetos sem diretriz vinculada
        public string? GuidelineId { get; set; }
        public string GuidelineTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Campaign { get; set; } = string.Empty;
        public int ProjectCount { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalFinancialReturn { get; set; }
        public decimal Roi { get; set; }
        public int ProductivityGainAverage { get; set; }
        public int ActiveProjects { get; set; }
        public int DelayedProjects { get; set; }
    }
}
