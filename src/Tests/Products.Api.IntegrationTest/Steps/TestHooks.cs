using Microsoft.Extensions.DependencyInjection;
using Products.Api.Data;
using Products.Api.IntegrationTest.Helper;
using Products.Api.Models;
using Reqnroll;
using System.Threading.Tasks;

namespace Products.Api.IntegrationTest.Steps
{
    [Binding]
    public class TestHooks
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly ScenarioContext _scenarioContext;

        // Constructor injection to access factory and context
        public TestHooks(CustomWebApplicationFactory factory, ScenarioContext scenarioContext)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Reset the test database before each scenario
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Clear existing data
            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.SaveChanges();

            // Seed with fresh test data
            SeedTestData(dbContext);

            // Store the factory in scenario context if needed elsewhere
            _scenarioContext["Factory"] = _factory;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            // Clean up any test artifacts or resources
            // For example, dispose of any resources created during the test
            if (_scenarioContext.ContainsKey("TestResources"))
            {
                var resources = _scenarioContext["TestResources"] as IDisposable;
                resources?.Dispose();
            }
        }

        private void SeedTestData(ApplicationDbContext dbContext)
        {
            dbContext.Products.AddRange(
                new Product { Name = "Test Product 1", Description = "Test Description 1", Price = 19.99m, Color = "Red", SKU = "TEST001", StockQuantity = 10 },
                new Product { Name = "Test Product 2", Description = "Test Description 2", Price = 29.99m, Color = "Blue", SKU = "TEST002", StockQuantity = 20 },
                new Product { Name = "Test Product 3", Description = "Test Description 3", Price = 39.99m, Color = "Red", SKU = "TEST003", StockQuantity = 30 }
            );
            

            dbContext.SaveChanges();
        }
    }
}