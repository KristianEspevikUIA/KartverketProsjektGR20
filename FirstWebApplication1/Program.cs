using FirstWebApplication1.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Authentication.Cookies; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configure identity options
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//rate limiter configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", opt =>
    {
        opt.PermitLimit = 10; // 10 requests
        opt.Window = TimeSpan.FromSeconds(10); // per 10 seconds
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5; // Allow queuing up to 5 requests
    });
});

// Henter connection string fra appsettings.json filen
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("The MariaDB connection string 'DefaultConnection' was not found.");
}

// Konfigurerer Entity Framework Core med MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 0))));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply pending EF Core migrations (creates Identity tables if needed)
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        // Seed roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "Pilot", "Registerfører" };

        foreach (var roleName in roleNames)
        {
            var exists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
            if (!exists)
            {
                var createResult = roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                if (!createResult.Succeeded)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Log and rethrow so startup fails visibly if migration/seed cannot complete.
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// --- Fix culture for macOS parsing of latitude/longitude ---
var defaultCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
// ------------------------------------------------------------

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
    
app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();