using Products.Api.Models;

namespace Products.Api.Events
{
    public interface IEventPublisher
    {
        Task PublishProductCreatedEventAsync(Product product);
        Task PublishProductUpdatedEventAsync(Product product);
    }
}
