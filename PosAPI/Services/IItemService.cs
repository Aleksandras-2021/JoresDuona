using PosShared;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Services;

public interface IItemService
{
    Task<PaginatedResult<Item>> GetAuthorizedItemsAsync(User sender,int pageNumber = 1, int pageSize = 10);
        /*
    Task<Item?> GetAuthorizedItemByIdAsync(int id, User sender);
    Task CreateItemAsync(ItemViewModel item, User sender);
    Task UpdateItemAsync(int id, ItemViewModel item, User sender);
    Task DeleteItemAsync(int id, User sender);
    Task<List<ItemVariation>> GetAuthorizedItemVariationsAsync(int id, User sender);
    Task<ItemVariation?> GetAuthorizedItemVariationByIdAsync(int varId, User sender);
    Task CreateItemVariationAsync(ItemVariation variation, User sender);
    Task UpdateItemVariationAsync(int varId, VariationsDTO variation, User sender);
    Task DeleteItemVariationAsync(int varId, User sender);
    */
}