using PosShared.Models;

namespace PosAPI.Repositories;

public interface IItemRepository
{
    // Items
    Task AddItemAsync(Item item);
    Task<List<Item>> GetAllItemsAsync();
    Task<List<Item>> GetAllBusinessItemsAsync(int businessId);
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
