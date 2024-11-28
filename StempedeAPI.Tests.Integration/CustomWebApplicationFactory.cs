using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Stemkit;
using Stemkit.Auth.Services.Interfaces;
using Stemkit.Data;
using Stemkit.Models;
using System.Linq;

namespace Stemkit.Tests.Integration
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing AppDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add AppDbContext with InMemory Database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (AppDbContext)
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    try
                    {
                        // Seed the database with test data if necessary
                        SeedData.Initialize(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test data. Error: {Message}", ex.Message);
                    }
                }

                // Mock ExternalAuthService if it's used by AuthController
                // Since you mentioned not needing to test ExternalAuthService, 
                // we'll mock it to satisfy the dependency injection without testing it.

                var externalAuthServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IExternalAuthService));

                if (externalAuthServiceDescriptor != null)
                {
                    services.Remove(externalAuthServiceDescriptor);
                }

                // Add a mock implementation that does nothing or returns default values
                var mockExternalAuthService = new Mock<IExternalAuthService>();
                // Setup default behaviors if necessary
                services.AddScoped<IExternalAuthService>(provider => mockExternalAuthService.Object);
            });
        }
    }

    // SeedData class to initialize test data
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Add roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleId = 1, RoleName = "Customer" },
                    new Role { RoleId = 2, RoleName = "Admin" }
                );
                context.SaveChanges();
            }

            // Add other necessary seed data as needed
        }
    }
}