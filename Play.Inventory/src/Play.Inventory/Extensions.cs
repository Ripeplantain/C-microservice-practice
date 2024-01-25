using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entites;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static IventoryItemDto AsDto(this InventoryItem item, string name, string description)
        {
            return new IventoryItemDto(
                item.CatalogItemId,
                name,
                description,
                item.Quantity,
                item.AcquiredDate
            );
        }
    }
}