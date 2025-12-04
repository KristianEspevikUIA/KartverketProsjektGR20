/// <summary>
/// Entry point for configuring services, middleware, security features, and the HTTP pipeline before
/// bootstrapping the ASP.NET Core MVC host. This file ties together the MVC (controllers/views) and
/// data (EF Core + Identity) layers of the application.
/// </summary>
using FirstWebApplication1.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

// Build a host builder that wires up dependency injection, configuration, and logging.
var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers and Razor views so routing/model binding can serve the UI layer.
builder.Services.AddControllersWithViews();

// ASP.NET Core Identity wires authentication/authorization into MVC. Cookie auth is used and the
// antiforgery system (ValidateAntiForgeryToken) guards form posts. Identity persists users/roles
// through ApplicationDbContext (EF Core) so SQL is parameterized and protected from injection.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configure password and sign-in policies; ModelState will reflect violations on account forms.
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Enables secure token flows (reset, confirmation)

// Rate limiter protects against brute-force/abuse by capping requests per user/IP in a time window.
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

// Pull the database connection string from configuration (appsettings/Secret Manager/env). EF Core will
// generate parameterized SQL to avoid injection, and Identity tables live in this database too.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("The MariaDB connection string 'DefaultConnection' was not found.");
}

// Register ApplicationDbContext with DI using MariaDB provider so controllers/services can request it.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 0))));

// Build the configured web application instance (runtime pipeline).
var app = builder.Build();

// Create a service scope so we can resolve scoped services (DbContext, managers) before the app runs.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply pending EF Core migrations (creates Identity tables if needed) at startup to keep schema up to date.
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(); // Changed to async

        // Seed roles - using async properly
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "Pilot", "Caseworker" };

        foreach (var roleName in roleNames)
        {
            var exists = await roleManager.RoleExistsAsync(roleName); // Async check keeps DB access non-blocking
            if (!exists)
            {
                var createResult = await roleManager.CreateAsync(new IdentityRole(roleName)); // Create missing roles for authZ
                if (!createResult.Succeeded)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // Seed admin user from configuration when credentials are provided
        var configuration = services.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var adminUser = await userManager.FindByEmailAsync(adminEmail); // Avoid duplicate seed

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword); // Password hashed and stored
                if (!createAdminResult.Succeeded)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Failed to create admin user {Email}: {Errors}", adminEmail, string.Join(", ", createAdminResult.Errors.Select(e => e.Description)));
                }
            }

            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin"); // Ensure admin permissions are present
            }
        }
        else
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Admin credentials not configured; admin user was not created.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database."); // Bubble up startup failures
        throw;
    }
}

// --- Fix culture for macOS parsing of latitude/longitude ---
var defaultCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
// ------------------------------------------------------------

// Configure the HTTP request pipeline (ordering matters for security/performance).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(); // Serves wwwroot assets with built-in XSS-safe static file middleware
app.UseHttpsRedirection(); // Force HTTPS to protect cookies and anti-forgery tokens
app.UseRouting(); // Enable endpoint routing for controllers

app.UseRateLimiter(); // Enforce configured rate limits globally

app.UseAuthentication(); // Issue/validate auth cookies for Identity

app.UseAuthorization(); // Enforce [Authorize] attributes in controllers

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();