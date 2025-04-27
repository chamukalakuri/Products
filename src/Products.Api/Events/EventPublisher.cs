using Azure.Messaging.ServiceBus;
using Products.Api.Models;
using System.Text.Json;

namespace Products.Api.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _productCreatedTopic;
        private readonly string _productUpdatedTopic;
        private readonly ILogger<EventPublisher> _logger;

        public EventPublisher(IConfiguration configuration, ILogger<EventPublisher> logger)
        {
            var connectionString = configuration["ServiceBus:ConnectionString"];
            _productCreatedTopic = configuration["ServiceBus:ProductCreatedTopic"] ?? "product-created";
            _productUpdatedTopic = configuration["ServiceBus:ProductUpdatedTopic"] ?? "product-updated";

            _serviceBusClient = new ServiceBusClient(connectionString);
            _logger = logger;
        }

        public async Task PublishProductCreatedEventAsync(Product product)
        {
            try
            {
                var sender = _serviceBusClient.CreateSender(_productCreatedTopic);
                var messageBody = JsonSerializer.Serialize(product);
                var message = new ServiceBusMessage(messageBody)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Subject = "ProductCreated",
                    ApplicationProperties =
                    {
                        { "ProductId", product.Id.ToString() },
                        { "ProductName", product.Name }
                    }
                };

                await sender.SendMessageAsync(message);
                _logger.LogInformation("Published ProductCreated event for product {ProductId}", product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ProductCreated event for product {ProductId}", product.Id);
                throw;
            }
        }

        public async Task PublishProductUpdatedEventAsync(Product product)
        {
            try
            {
                var sender = _serviceBusClient.CreateSender(_productUpdatedTopic);
                var messageBody = JsonSerializer.Serialize(product);
                var message = new ServiceBusMessage(messageBody)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Subject = "ProductUpdated",
                    ApplicationProperties =
                    {
                        { "ProductId", product.Id.ToString() },
                        { "ProductName", product.Name }
                    }
                };

                await sender.SendMessageAsync(message);
                _logger.LogInformation("Published ProductUpdated event for product {ProductId}", product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ProductUpdated event for product {ProductId}", product.Id);
                throw;
            }
        }
    }
}