using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoSecApp.Web.Data;
using InfoSecApp.Web.Models;

namespace InfoSecApp.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class SuperAdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SuperAdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants()
    {
        var tenants = await _context.Tenants.ToListAsync();
        return Ok(tenants);
    }

    [HttpGet("tenants/{id}")]
    public async Task<ActionResult<Tenant>> GetTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound();
        }

        return Ok(tenant);
    }

    [HttpPost("tenants")]
    public async Task<ActionResult<Tenant>> CreateTenant(CreateTenantRequest request)
    {
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == request.Identifier);

        if (existingTenant != null)
        {
            return BadRequest("Tenant with this identifier already exists.");
        }

        var tenant = new Tenant
        {
            Name = request.Name,
            Identifier = request.Identifier,
            ConnectionString = request.ConnectionString,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
    }

    [HttpPut("tenants/{id}")]
    public async Task<IActionResult> UpdateTenant(int id, UpdateTenantRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound();
        }

        tenant.Name = request.Name;
        tenant.IsActive = request.IsActive;
        tenant.ConnectionString = request.ConnectionString;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("tenants/{id}")]
    public async Task<IActionResult> DeleteTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return NotFound();
        }

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userManager.Users
            .Include(u => u.Tenant)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                TenantId = u.TenantId,
                TenantName = u.Tenant != null ? u.Tenant.Name : null,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("users/{userId}/assign-role")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
        if (!roleExists)
        {
            return BadRequest("Role does not exist.");
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = $"Role '{request.RoleName}' assigned to user." });
    }
}

public class CreateTenantRequest
{
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public string? ConnectionString { get; set; }
}

public class UpdateTenantRequest
{
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public string? ConnectionString { get; set; }
}

public class UserDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignRoleRequest
{
    public required string RoleName { get; set; }
}
