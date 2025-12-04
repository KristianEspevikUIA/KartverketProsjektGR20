using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using FirstWebApplication1.Data;

namespace FirstWebApplication1.Services
{
    public class MigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<MigrationHostedService> _logger;

        public MigrationHostedService(IServiceProvider services, ILogger<MigrationHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration service...");

            // Run migrations in background without blocking app startup
            _ = Task.Run(async () =>
            {
                try
                {
                    // Wait a bit for database to be ready
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    
                    await RunMigrationsWithRetryAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background migration task failed");
                }
            }, cancellationToken);
        }

        private async Task RunMigrationsWithRetryAsync(CancellationToken cancellationToken)
        {
            var maxAttempts = 10;
            var delay = TimeSpan.FromSeconds(5);

            for (int attempt = 1; attempt <= maxAttempts && !cancellationToken.IsCancellationRequested; attempt++)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    _logger.LogInformation("Attempting database migration (attempt {Attempt}/{Max})...", attempt, maxAttempts);
                    
                    // Test connection first
                    await db.Database.OpenConnectionAsync(cancellationToken);
                    await db.Database.CloseConnectionAsync();
                    
                    // Apply migrations
                    await db.Database.MigrateAsync(cancellationToken);
                    
                    _logger.LogInformation("Database migrations applied successfully!");

                    // Seed data after successful migration
                    await SeedDataAsync(scope, cancellationToken);
                    
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Migration cancelled by application shutdown.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Migration attempt {Attempt} failed.", attempt);
                    
                    if (attempt == maxAttempts)
                    {
                        _logger.LogError(ex, "All migration attempts failed. Application will continue without database setup.");
                        return; // Don't crash the app
                    }

                    _logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30)); // Exponential backoff with max 30s
                }
            }
        }

        private async Task SeedDataAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // Seed roles
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin", "Pilot", "Caseworker" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        _logger.LogInformation("Created role: {Role}", roleName);
                    }
                }

                // Seed admin user from configuration
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var adminEmail = configuration["Admin:Email"];
                var adminPassword = configuration["Admin:Password"];

                if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);

                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(adminUser, adminPassword);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Created admin user: {Email}", adminEmail);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to create admin user: {Errors}", 
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }

                    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        _logger.LogInformation("Added admin role to user: {Email}", adminEmail);
                    }
                }

                _logger.LogInformation("Database seeding completed!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping database migration service...");
            return Task.CompletedTask;
        }
    }
}