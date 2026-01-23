using Microsoft.AspNetCore.Identity;

namespace InfoSecApp.Web.Models;

public class ApplicationUser : IdentityUser
{
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
