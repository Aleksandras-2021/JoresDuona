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

}