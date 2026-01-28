using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InfoSecApp.Web.Data;
using InfoSecApp.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Set to false for easier testing
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialize database and seed data with retry logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Retry configuration
    int maxRetries = 10;
    int delayMilliseconds = 5000; // Start with 5 seconds
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to initialize database (attempt {Attempt}/{MaxRetries})...", i + 1, maxRetries);
            
            // Ensure database is created and migrations are applied
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            
            // Seed initial data
            await DbInitializer.Initialize(services);
            
            logger.LogInformation("Database initialized successfully.");
            break;
        }
        catch (Exception ex)
        {
            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "Failed to initialize database after {MaxRetries} attempts.", maxRetries);
                throw; // Re-throw on final attempt
            }
            
            logger.LogWarning(ex, "Database initialization attempt {Attempt} failed. Retrying in {Delay}ms...", i + 1, delayMilliseconds);
            await Task.Delay(delayMilliseconds);
            
            // Exponential backoff with cap at 30 seconds
            delayMilliseconds = Math.Min(delayMilliseconds * 2, 30000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
