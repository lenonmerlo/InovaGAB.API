namespace InovaGAB.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string Division { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    }

    public enum UserRole
    {
        Employee,
        Manager,
        Leader
    }
}
