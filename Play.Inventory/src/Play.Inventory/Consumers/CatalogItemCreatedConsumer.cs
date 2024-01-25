using MassTransit;
using Play.Catalog.Contracts;
using Play.Inventory.Service.Entities;
using Play.Common;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {

        private readonly IRespository<CatalogItem> repository;

        public CatalogItemCreatedConsumer(IRespository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if (item != null)
            {
                return;
            }

            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await repository.CreateAsync(item);
            
        }
    }
}