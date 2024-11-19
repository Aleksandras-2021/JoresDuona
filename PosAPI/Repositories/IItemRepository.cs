using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IItemRepository
    {
        Task<List<Item>> GetAllItemsAsync();
        Task<List<Item>> GetAllBusinessItemsAsync(int businessId);
        Task<Item> GetItemByIdAsync(int id);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(int id);
    }
}
