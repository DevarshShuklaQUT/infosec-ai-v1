namespace InfoSecApp.Web.Models;

public class Tenant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public string? ConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
