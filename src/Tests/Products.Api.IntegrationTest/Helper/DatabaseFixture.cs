using Microsoft.Extensions.DependencyInjection;
using Products.Api.Data;

namespace Products.Api.IntegrationTest.Helper
{
    public class DatabaseFixture : IDisposable
    {
        public CustomWebApplicationFactory Factory { get; }
        public HttpClient Client { get; }
        public ApplicationDbContext Context { get; }

        private readonly IServiceScope _scope;

        public DatabaseFixture()
        {
            Factory = new CustomWebApplicationFactory();
            Client = Factory.CreateClient();

            _scope = Factory.Services.CreateScope();
            Context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            ResetDatabase();
            SeedTestData();
        }

        private void ResetDatabase()
        {
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }

        private void SeedTestData()
        {
            Context.Products.AddRange(
                new Models.Product
                {
                    Name = "Test Red Product",
                    Description = "Test Red Product Description",
                    Price = 9.99m,
                    Color = "Red",
                    SKU = "TEST-RED-001",
                    StockQuantity = 10
                },
                new Models.Product
                {
                    Name = "Test Blue Product",
                    Description = "Test Blue Product Description",
                    Price = 19.99m,
                    Color = "Blue",
                    SKU = "TEST-BLUE-001",
                    StockQuantity = 20
                }
            );
            Context.SaveChanges();
        }

        public void Dispose()
        {
            _scope.Dispose();
            Client.Dispose();
            Factory.Dispose();
        }
    }
}