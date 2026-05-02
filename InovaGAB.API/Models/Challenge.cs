// Models/Challenge.cs
namespace InovaGAB.API.Models;

public class Challenge
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Prize { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}