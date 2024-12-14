using PosShared;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Services;

public interface IItemService
{
    Task<PaginatedResult<Item>> GetAuthorizedItemsAsync(User sender,int pageNumber = 1, int pageSize = 10);
        
    Task<Item?> GetAuthorizedItemByIdAsync(int id, User? sender);
    Task<Item> CreateAuthorizedItemAsync(ItemViewModel item, User? sender);
    Task UpdateAuthorizedItemAsync(int itemId,ItemViewModel item, User? sender);
    Task DeleteAuthorizedItemAsync(int itemId, User sender);
    
    //Variations
    Task<ItemVariation> CreateAuthorizedItemVariationAsync(ItemVariation variation, User? sender);
    Task UpdateAuthorizedItemVariationAsync(int varId,VariationsDTO variation, User? sender);
    Task DeleteAuthorizedItemVariationAsync(int varId, User? sender);

    Task<List<ItemVariation>> GetAuthorizedItemVariationsAsync(int id, User? sender);
    Task<ItemVariation?> GetAuthorizedItemVariationByIdAsync(int varId, User? sender);
    
}