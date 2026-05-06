using System.ComponentModel.DataAnnotations.Schema;

namespace InovaGAB.API.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    [Column("UserEmail")]
    public string? UserEmail { get; set; }
    [Column("UserRole")]
    public string? UserRole { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}