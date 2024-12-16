using Microsoft.AspNetCore.Razor.Hosting;
using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Services;

public class ItemService: IItemService
{
    private readonly IItemRepository _itemRepository;
    
    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }
    
    public async Task<PaginatedResult<Item>> GetAuthorizedItemsAsync(User sender, int pageNumber = 1, int pageSize = 10)
    {
        AuthorizationHelper.Authorize("Items", "List", sender);
        
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
        AuthorizationHelper.Authorize("Items", "Read", sender);
        
        var item = await _itemRepository.GetItemByIdAsync(id);

        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId,sender.BusinessId,"Read");

        return item;
    }

    public async Task<Item> CreateAuthorizedItemAsync(ItemViewModel item, User sender)
    {
        AuthorizationHelper.Authorize("Items", "Create", sender);
        
        Item newItem = new Item();

        newItem.BusinessId = sender.BusinessId;
        newItem.Name = item.Name;
        newItem.Description = item.Description;
        newItem.Price = item.Price;
        newItem.BasePrice = item.BasePrice;
        newItem.Category = item.Category;
        newItem.Quantity = item.Quantity;

        await _itemRepository.AddItemAsync(newItem);

        return newItem;
    }

    public async Task UpdateAuthorizedItemAsync(int id,ItemViewModel item, User sender)
    {
        AuthorizationHelper.Authorize("Items", "Update", sender);

        var existingItem = await _itemRepository.GetItemByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,existingItem.BusinessId ,sender.BusinessId, "Update");

        
        existingItem.Price = item.Price;
        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.BasePrice = item.BasePrice;
        existingItem.Category = item.Category;
        existingItem.Quantity = item.Quantity;
        
        await _itemRepository.UpdateItemAsync(existingItem);
    }

    public async Task DeleteAuthorizedItemAsync(int itemId, User sender)
    {
        AuthorizationHelper.Authorize("Items", "Delete", sender);
        var existingItem = await _itemRepository.GetItemByIdAsync(itemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,existingItem.BusinessId ,sender.BusinessId, "Delete");
        await _itemRepository.DeleteItemAsync(itemId);
    }

    public async Task<ItemVariation> CreateAuthorizedItemVariationAsync(ItemVariation variation, User? sender)
    {
        AuthorizationHelper.Authorize("Items", "Create", sender);
        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Create");
        
        var newVariation = new ItemVariation
        {
            ItemId = item.Id,
            Name = variation.Name,
            AdditionalPrice = variation.AdditionalPrice,
        };
        await _itemRepository.AddItemVariationAsync(newVariation);

        return newVariation;
    }

    public async Task UpdateAuthorizedItemVariationAsync(int varId,VariationsDTO variation, User? sender)
    {
        AuthorizationHelper.Authorize("Items", "Update", sender);
        ItemVariation? existingVariation = await _itemRepository.GetItemVariationByIdAsync(varId);
        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Update");

        existingVariation.AdditionalPrice = variation.AdditionalPrice;
        existingVariation.Name = variation.Name;
        existingVariation.ItemId = variation.ItemId;
        
        await _itemRepository.UpdateItemVariationAsync(existingVariation);
    }

    public async Task DeleteAuthorizedItemVariationAsync(int varId, User? sender)
    {
        AuthorizationHelper.Authorize("Items", "Delete", sender);
        var variation = await _itemRepository.GetItemVariationByIdAsync(varId);
        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Delete");
        
        await _itemRepository.DeleteItemVariationAsync(varId);
    }


    public async Task<ItemVariation?> GetAuthorizedItemVariationByIdAsync(int varId, User? sender)
    {
        AuthorizationHelper.Authorize("Items", "Read", sender);
        var variation = await _itemRepository.GetItemVariationByIdAsync(varId);
        var item = await  _itemRepository.GetItemByIdAsync(variation.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Read");
        
        return variation;
    }
    
    public async Task<List<ItemVariation>> GetAuthorizedItemVariationsAsync(int id, User sender)
    {
        AuthorizationHelper.Authorize("Items", "List", sender);
        var item = await _itemRepository.GetItemByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "List");
        
        var variations = await _itemRepository.GetItemVariationsAsync(id);
        
        if(!variations.Any())
            return (new List<ItemVariation>());
        
        return variations;
    }
}