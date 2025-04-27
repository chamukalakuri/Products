using Products.Api.Events;
using Products.Api.Models;
using Products.Api.Repositories;

namespace Products.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IEventPublisher eventPublisher,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByColorAsync(string color)
        {
            var products = await _productRepository.GetProductsByColorAsync(color);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Color = productDto.Color,
                SKU = productDto.SKU,
                StockQuantity = productDto.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.CreateProductAsync(product);

            // Publish event
            try
            {
                await _eventPublisher.PublishProductCreatedEventAsync(createdProduct);
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                _logger.LogWarning(ex, "Failed to publish product created event for product {ProductId}", createdProduct.Id);
            }

            return MapToDto(createdProduct);
        }

        public async Task UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);

            if (existingProduct == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Color = productDto.Color;
            existingProduct.SKU = productDto.SKU;
            existingProduct.StockQuantity = productDto.StockQuantity;

            await _productRepository.UpdateProductAsync(existingProduct);

            // Publish event
            try
            {
                await _eventPublisher.PublishProductUpdatedEventAsync(existingProduct);
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                _logger.LogWarning(ex, "Failed to publish product updated event for product {ProductId}", existingProduct.Id);
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteProductAsync(id);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Color = product.Color,
                SKU = product.SKU,
                StockQuantity = product.StockQuantity
            };
        }
    }
}