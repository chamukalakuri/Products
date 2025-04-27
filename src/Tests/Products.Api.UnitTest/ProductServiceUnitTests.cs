using Microsoft.Extensions.Logging;
using Moq;
using Products.Api.Events;
using Products.Api.Models;
using Products.Api.Repositories;
using Products.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Api.UnitTest
{
    public class ProductServiceUnitTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<IEventPublisher> _mockEventPublisher;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly ProductService _service;
        private readonly List<Product> _products;

        public ProductServiceUnitTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _service = new ProductService(_mockRepo.Object, _mockEventPublisher.Object, _mockLogger.Object);

            // Test data
            _products = new List<Product>
            {
                new Product { Id = 1, Name = "Test Product 1", Description = "Description 1", Price = 19.99m, Color = "Red", SKU = "SKU001", StockQuantity = 10 },
                new Product { Id = 2, Name = "Test Product 2", Description = "Description 2", Price = 29.99m, Color = "Blue", SKU = "SKU002", StockQuantity = 20 },
                new Product { Id = 3, Name = "Test Product 3", Description = "Description 3", Price = 39.99m, Color = "Red", SKU = "SKU003", StockQuantity = 30 }
            };
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync(_products);

            // Act
            var result = await _service.GetAllProductsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal("Test Product 1", result.First().Name);
        }

        [Fact]
        public async Task GetProductsByColorAsync_ShouldReturnProductsWithMatchingColor()
        {
            // Arrange
            string color = "Red";
            var redProducts = _products.Where(p => p.Color == color).ToList();

            _mockRepo.Setup(repo => repo.GetProductsByColorAsync(color))
                .ReturnsAsync(redProducts);

            // Act
            var result = await _service.GetProductsByColorAsync(color);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, item => Assert.Equal("Red", item.Color));
        }

        [Fact]
        public async Task CreateProductAsync_ShouldReturnCreatedProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Description",
                Price = 49.99m,
                Color = "Green",
                SKU = "SKU004",
                StockQuantity = 40
            };

            var newProduct = new Product
            {
                Id = 4,
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                Color = createDto.Color,
                SKU = createDto.SKU,
                StockQuantity = createDto.StockQuantity
            };

            _mockRepo.Setup(x => x.CreateProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(newProduct);

            _mockEventPublisher.Setup(x => x.PublishProductCreatedEventAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateProductAsync(createDto);

            // Assert
            Assert.Equal(4, result.Id);
            Assert.Equal("New Product", result.Name);
            Assert.Equal("Green", result.Color);
            _mockEventPublisher.Verify(x => x.PublishProductCreatedEventAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidId_ShouldUpdateProduct()
        {
            // Arrange
            int productId = 1;
            var updateDto = new UpdateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 59.99m,
                Color = "Yellow",
                SKU = "SKU001-UPD",
                StockQuantity = 50
            };

            _mockRepo.Setup(repo => repo.GetProductByIdAsync(productId))
                .ReturnsAsync(_products.First());

            _mockEventPublisher.Setup(x => x.PublishProductUpdatedEventAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await _service.UpdateProductAsync(productId, updateDto);

            // Verify the repository was called with the updated product
            _mockRepo.Verify(repo => repo.UpdateProductAsync(It.Is<Product>(
                p => p.Id == productId &&
                p.Name == updateDto.Name &&
                p.Color == updateDto.Color
            )), Times.Once);

            _mockEventPublisher.Verify(x => x.PublishProductUpdatedEventAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int invalidId = 999;
            var updateDto = new UpdateProductDto();

            _mockRepo.Setup(repo => repo.GetProductByIdAsync(invalidId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateProductAsync(invalidId, updateDto));
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldCallRepositoryDelete()
        {
            // Arrange
            int productId = 1;

            // Act
            await _service.DeleteProductAsync(productId);

            // Assert
            _mockRepo.Verify(repo => repo.DeleteProductAsync(productId), Times.Once);
        }
    }
}