using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly IUserTokenService _userTokenService;
    private readonly IItemService _itemService;
    public ItemsController(ILogger<ItemsController> logger, IItemRepository itemRepository, 
        IUserTokenService userTokenService,IItemService itemService)
    {
        _logger = logger;
        _userTokenService = userTokenService;
        _itemService = itemService;
    }
    
    // GET: api/Items
    [HttpGet]
    public async Task<IActionResult> GetAllItems(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var paginatedItems = await _itemService.GetAuthorizedItemsAsync(sender, pageNumber, pageSize);

        if (paginatedItems.Items.Count > 0)
            return Ok(paginatedItems);
        else
            return NotFound("No Items found.");
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Item? item = await _itemService.GetAuthorizedItemByIdAsync(id, sender);
        return Ok(item);
    }

    // POST: api/Items
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] ItemViewModel item)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var newItem = await _itemService.CreateAuthorizedItemAsync(item,sender);
        return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
    }
    
    // PUT: api/Items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemViewModel item)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.UpdateAuthorizedItemAsync(id,item, sender);
        return Ok();
    }
    
    // DELETE: api/Items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.DeleteAuthorizedItemAsync(id,sender);
        _logger.LogInformation($"User with id {sender.Id} deleted item with id {id} at {DateTime.Now}");
        return Ok();
    }

    // GET: api/Items/{id}/Variations
    [HttpGet("{id}/Variations")]
    public async Task<IActionResult> GetAllItemVariations(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        List<ItemVariation> variations = await _itemService.GetAuthorizedItemVariationsAsync(id, sender);
        return Ok(variations);
    }

    // GET: api/Items/Variations{varId}
    [HttpGet("Variations/{varId}")]
    public async Task<IActionResult> GetItemVariationById(int varId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        ItemVariation? variation = await _itemService.GetAuthorizedItemVariationByIdAsync(varId,sender);

        VariationsDTO variationDTO = new VariationsDTO
        {
            AdditionalPrice = variation.AdditionalPrice,
            Name = variation.Name,
            Id = variation.Id,
            ItemId = variation.ItemId
        };
            return Ok(variationDTO);
    }

    // POST: api/Items/id/Variations
    [HttpPost("{id}/Variations")]
    public async Task<IActionResult> CreateVariation([FromBody] ItemVariation variation)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var newVariation = await _itemService.CreateAuthorizedItemVariationAsync(variation, sender);
        return CreatedAtAction(
            nameof(GetItemVariationById),
            new { id = newVariation.Id, varId = newVariation.Id },
            newVariation
            );
    }


    // DELETE: api/Items/Variations/{id}
    [HttpDelete("Variations/{varId}")]
    public async Task<IActionResult> DeleteVariation(int varId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.DeleteAuthorizedItemVariationAsync(varId, sender);
        return Ok();
    }

    // PUT: api/Items/Variations/{id}
    [HttpPut("Variations/{id}")]
    public async Task<IActionResult> UpdateVariation(int id, VariationsDTO variation)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.UpdateAuthorizedItemVariationAsync(id,variation,sender);
        return Ok();
    }
    
}
