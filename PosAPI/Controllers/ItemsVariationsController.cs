using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/Items")]
[ApiController]
public class ItemsVariations : ControllerBase
{
    private readonly IUserTokenService _userTokenService;
    private readonly IItemService _itemService;
    
    public ItemsVariations(IUserTokenService userTokenService,IItemService itemService)
    {
        _userTokenService = userTokenService;
        _itemService = itemService;
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
