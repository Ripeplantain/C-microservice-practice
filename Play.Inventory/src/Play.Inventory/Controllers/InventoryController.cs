using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entites;
using Play.Inventory.Service.Entities;


namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IRespository<InventoryItem> itemsRepository;
        private readonly IRespository<CatalogItem> catalogRespository;

        public InventoryController(
            IRespository<InventoryItem> itemsRepository, IRespository<CatalogItem> catalogRespository)
        {
            this.itemsRepository = itemsRepository;
            this.catalogRespository = catalogRespository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.UserId == userId);
            var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
            var catalogItems = await catalogRespository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem => 
            {
                var catalogItem = catalogItems.SingleOrDefault(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);

                if (catalogItem is null)
                {
                    throw new Exception($"Catalog item with id {inventoryItem.CatalogItemId} not found");
                }
                else
                {
                    return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
                }
            });

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await itemsRepository.GetAsync(
                item => item.UserId == grantItemDto.UserId && item.CatalogItemId == grantItemDto.CatalogItemId
            );

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemDto.CatalogItemId,
                    UserId = grantItemDto.UserId,
                    Quantity = grantItemDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemDto.Quantity;
                await itemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}