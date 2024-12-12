using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories;

public interface IItemRepository
{
    // Items
    Task AddItemAsync(Item item);
    Task<PaginatedResult<Item>> GetAllItemsAsync(int pageNumber, int pageSize);
    Task<PaginatedResult<Item>> GetAllBusinessItemsAsync(int businessId, int pageNumber, int pageSize);
    Task<Item> GetItemByIdAsync(int id);
    Task UpdateItemAsync(Item item);
    Task DeleteItemAsync(int id);

    // Item Variations
    Task<List<ItemVariation>> GetItemVariationsAsync(int itemId);
    Task<ItemVariation> GetItemVariationByIdAsync(int variationId);
    Task AddItemVariationAsync(ItemVariation itemVariation);
    Task UpdateItemVariationAsync(ItemVariation itemVariation);
    Task DeleteItemVariationAsync(int variationId);
}
