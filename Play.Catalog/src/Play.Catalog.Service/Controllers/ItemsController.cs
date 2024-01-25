using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entites;
using Play.Common;
using Play.Catalog.Contracts;


namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly IRespository<Item> itemRepository;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemController(IRespository<Item> itemRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemRepository = itemRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetAsync()
        {
            var items  = (await itemRepository.GetAllAsync())
                            .Select(item => item.AsDto());

            return items;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemRepository.CreateAsync(item);

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }
            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;
            await itemRepository.UpdateAsync(existingItem);

            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingItem = await itemRepository.GetAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }
            await itemRepository.RemoveAsync(id);

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}