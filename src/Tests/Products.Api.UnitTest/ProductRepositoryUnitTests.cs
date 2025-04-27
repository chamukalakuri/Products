using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Models;
using Products.Api.Repositories;
using Xunit;

namespace Products.Api.UnitTest
{
    public class ProductRepositoryUnitTests
    {
            private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

            public ProductRepositoryUnitTests()
            {
                // Configure the in-memory database with a unique name to avoid conflicts between tests
                _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(databaseName: $"ProductsDb_Test_{Guid.NewGuid()}")
                    .Options;
            }

            private async Task<ApplicationDbContext> CreateAndSeedDbContextAsync()
            {
                var context = new ApplicationDbContext(_dbContextOptions);
                await context.Database.EnsureCreatedAsync();

                // Clear the database to ensure a clean slate
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                // Seed with test data
                context.Products.AddRange(
                    new Product { Id = 1, Name = "Test Product 1", Description = "Description 1", Price = 19.99m, Color = "Red", SKU = "SKU001", StockQuantity = 10, CreatedAt = DateTime.UtcNow },
                    new Product { Id = 2, Name = "Test Product 2", Description = "Description 2", Price = 29.99m, Color = "Blue", SKU = "SKU002", StockQuantity = 20, CreatedAt = DateTime.UtcNow },
                    new Product { Id = 3, Name = "Test Product 3", Description = "Description 3", Price = 39.99m, Color = "Red", SKU = "SKU003", StockQuantity = 30, CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();

                return context;
            }

            #region GetAllProductsAsync Tests

            [Fact]
            public async Task GetAllProductsAsync_ShouldReturnAllProducts()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);

                // Act
                var products = await repository.GetAllProductsAsync();

                // Assert
                Assert.Equal(3, products.Count());
                Assert.Contains(products, p => p.Name == "Test Product 1");
                Assert.Contains(products, p => p.Name == "Test Product 2");
                Assert.Contains(products, p => p.Name == "Test Product 3");
            }

            [Fact]
            public async Task GetAllProductsAsync_ShouldReturnEmptyList_WhenNoProductsExist()
            {
                // Arrange
                await using var context = new ApplicationDbContext(_dbContextOptions);
                await context.Database.EnsureCreatedAsync();
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                var repository = new ProductRepository(context);

                // Act
                var products = await repository.GetAllProductsAsync();

                // Assert
                Assert.Empty(products);
            }

            #endregion

            #region GetProductsByColorAsync Tests

            [Fact]
            public async Task GetProductsByColorAsync_ShouldReturnProductsWithMatchingColor()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);
                string color = "Red";

                // Act
                var products = await repository.GetProductsByColorAsync(color);

                // Assert
                Assert.Equal(2, products.Count());
                Assert.All(products, p => Assert.Equal("Red", p.Color));
            }

            [Fact]
            public async Task GetProductsByColorAsync_ShouldReturnEmptyList_WhenNoProductsWithColorExist()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);
                string color = "Green";

                // Act
                var products = await repository.GetProductsByColorAsync(color);

                // Assert
                Assert.Empty(products);
            }

            #endregion

            #region GetProductByIdAsync Tests

            [Fact]
            public async Task GetProductByIdAsync_ShouldReturnProduct_WhenItExists()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);
                int productId = 1;

                // Act
                var product = await repository.GetProductByIdAsync(productId);

                // Assert
                Assert.NotNull(product);
                Assert.Equal(productId, product.Id);
                Assert.Equal("Test Product 1", product.Name);
                Assert.Equal("Red", product.Color);
            }

            [Fact]
            public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);
                int nonExistentProductId = 999;

                // Act
                var product = await repository.GetProductByIdAsync(nonExistentProductId);

                // Assert
                Assert.Null(product);
            }

            #endregion

            #region CreateProductAsync Tests

            [Fact]
            public async Task CreateProductAsync_ShouldAddProductToDatabase()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);
                var newProduct = new Product
                {
                    Name = "New Product",
                    Description = "New Description",
                    Price = 49.99m,
                    Color = "Green",
                    SKU = "SKU004",
                    StockQuantity = 40
                };

                // Act
                var createdProduct = await repository.CreateProductAsync(newProduct);

                // Assert
                Assert.NotEqual(0, createdProduct.Id); // Should have an ID assigned

                // Verify product was added to the database
                var productInDb = await context.Products.FindAsync(createdProduct.Id);
                Assert.NotNull(productInDb);
                Assert.Equal("New Product", productInDb.Name);
                Assert.Equal("Green", productInDb.Color);
            }

            #endregion

            #region UpdateProductAsync Tests

            [Fact]
            public async Task UpdateProductAsync_ShouldUpdateProductInDatabase()
            {
                // Arrange
                await using var context = await CreateAndSeedDbContextAsync();
                var repository = new ProductRepository(context);

                // Get existing product
                var existingProduct = await context.Products.FindAsync(1);
                Assert.NotNull(existingProduct);

                // Modify product
                existingProduct.Name = "Updated Product";
                existingProduct.Price = 99.99m;
                existingProduct.Color = "Yellow";

                // Act
                await repository.UpdateProductAsync(existingProduct);

                // Assert - Get fresh instance from DB
                await using var newContext = new ApplicationDbContext(_dbContextOptions);
                var updatedProduct = await newContext.Products.FindAsync(1);

                Assert.NotNull(updatedProduct);
                Assert.Equal("Updated Product", updatedProduct.Name);
                Assert.Equal(99.99m, updatedProduct.Price);
                Assert.Equal("Yellow", updatedProduct.Color);
            }

            #endregion
        }
    }