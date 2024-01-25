using MassTransit;
using Play.Catalog.Contracts;
using Play.Inventory.Service.Entities;
using Play.Common;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeteledConsumer : IConsumer<CatalogItemDeleted>
    {

        private readonly IRespository<CatalogItem> repository;

        public CatalogItemDeteledConsumer(IRespository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if (item == null)
            {
                return;
            }

            await repository.RemoveAsync(item.Id);
        }
    }
}