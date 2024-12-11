using PosAPI.Repositories;
using PosShared;
using PosShared.Models;

namespace PosAPI.Services;

public class ItemService: IItemService
{
    private readonly IItemRepository _itemRepository;
    
    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }
    
    public async Task<PaginatedResult<Item>> GetAuthorizedItemsAsync(User sender, 
        int pageNumber = 1,
        int pageSize = 10)
    {
        PaginatedResult<Item> items = null;

        if (sender.Role == UserRole.SuperAdmin)
            items = await _itemRepository.GetAllItemsAsync(pageNumber,pageSize);
        else if (sender.Role == UserRole.Manager ||
                 sender.Role == UserRole.Owner ||
                 sender.Role == UserRole.Worker)
            items = await _itemRepository.GetAllBusinessItemsAsync(sender.BusinessId, pageNumber, pageSize);
        else
            items = PaginatedResult<Item>.Create(new List<Item>(), 0, pageNumber, pageSize);

        return items;
    }

    public async Task<Item?> GetAuthorizedItemByIdAsync(int id, User sender)
    {
        var item = await _itemRepository.GetItemByIdAsync(id);

        if (sender.Role != UserRole.SuperAdmin && item.BusinessId != sender.BusinessId)
            throw new UnauthorizedAccessException();

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {id} not found.");

        return item;
    }
    
    public async Task<ItemVariation?> GetAuthorizedItemVariationByIdAsync(int varId, User sender)
    {
        var variation = await _itemRepository.GetItemVariationByIdAsync(varId);
        var item = await  _itemRepository.GetItemByIdAsync(variation.ItemId);
        
        if (sender.Role != UserRole.SuperAdmin && item.BusinessId != sender.BusinessId)
            throw new UnauthorizedAccessException();

        if (variation == null)
            throw new KeyNotFoundException($"Variation with ID {varId} not found.");

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {variation.ItemId} not found.");

        return variation;
    }

    public async Task<List<ItemVariation>> GetAuthorizedItemVariationsAsync(int id, User sender)
    {
        var item = await _itemRepository.GetItemByIdAsync(id);

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {id} not found.");

        if (item.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this item.");

        var variations = await _itemRepository.GetItemVariationsAsync(id);
        
        if(!variations.Any())
            throw new KeyNotFoundException($"No variations for Item with ID {id} found.");
        
        return variations;
    }
}