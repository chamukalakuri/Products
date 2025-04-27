using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Products.Api.Data;
using Products.Api.Events;
using Products.Api.Models;
using Products.Api.Repositories;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Products.Api.IntegrationTest.Helper
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Override configuration to use test settings
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=ProductsDb_Test;Trusted_Connection=True;MultipleActiveResultSets=true"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace the event publisher
                services.RemoveAll<IEventPublisher>();
                services.AddScoped<IEventPublisher, TestEventPublisher>();

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Build the service provider to get the DbContext
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            });
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            // Add a test product 
            context.Products.AddRange(
                   new Product { Name = "Test Product 1", Description = "Test Description 1", Price = 19.99m, Color = "Red", SKU = "TEST001", StockQuantity = 10 },
                   new Product { Name = "Test Product 2", Description = "Test Description 2", Price = 29.99m, Color = "Blue", SKU = "TEST002", StockQuantity = 20 },
                   new Product { Name = "Test Product 3", Description = "Test Description 3", Price = 39.99m, Color = "Red", SKU = "TEST003", StockQuantity = 30 }
               );
            
            context.SaveChanges();
        }
    }
        // Helper extension method
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection RemoveAll<TService>(this IServiceCollection services)
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
                return services;
            }
        }

    // Test authentication handler for integration tests
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock)
                : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                // Create a test identity for integration tests
                var claims = new[] { new Claim(ClaimTypes.Name, "Test User") };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Test");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }

        // Test implementation of IEventPublisher to avoid real Service Bus calls
        public class TestEventPublisher : IEventPublisher
        {
            private readonly ILogger<TestEventPublisher> _logger;

            public TestEventPublisher(ILogger<TestEventPublisher> logger)
            {
                _logger = logger;
            }

            public Task PublishProductCreatedEventAsync(Product product)
            {
                _logger.LogInformation("Test: Publishing ProductCreated event for product {ProductId}", product.Id);
                return Task.CompletedTask;
            }

            public Task PublishProductUpdatedEventAsync(Product product)
            {
                _logger.LogInformation("Test: Publishing ProductUpdated event for product {ProductId}", product.Id);
                return Task.CompletedTask;
            }
        }
    }