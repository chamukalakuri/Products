using Products.Api.Models;
using System.Text.Json;

namespace Products.Api.Events
{
    public class MockEventPublisher : IEventPublisher
    {
        private readonly ILogger<MockEventPublisher> _logger;

        public MockEventPublisher(ILogger<MockEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishProductCreatedEventAsync(Product product)
        {
            // Simulate sending a message
            var messageBody = JsonSerializer.Serialize(product);

            _logger.LogInformation("[MOCK] Published ProductCreated event: {MessageBody}", messageBody);
            _logger.LogInformation("[MOCK] Metadata: ProductId={ProductId}, ProductName={ProductName}", product.Id, product.Name);

            return Task.CompletedTask;
        }

        public Task PublishProductUpdatedEventAsync(Product product)
        {
            // Simulate sending a message
            var messageBody = JsonSerializer.Serialize(product);

            _logger.LogInformation("[MOCK] Published ProductUpdated event: {MessageBody}", messageBody);
            _logger.LogInformation("[MOCK] Metadata: ProductId={ProductId}, ProductName={ProductName}", product.Id, product.Name);

            return Task.CompletedTask;
        }
    }
}
