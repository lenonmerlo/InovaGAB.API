namespace InovaGAB.API.DTOs.Request;

public class ApproveIdeaRequest
{
    public int ImpactScore { get; set; }
    public int FeasibilityScore { get; set; }
    public int AlignmentScore { get; set; }
}