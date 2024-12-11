using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosAPI.Migrations;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly IItemRepository _itemRepository;
    private readonly IUserRepository _userRepository;
    private readonly IItemService _itemService;
    public ItemsController(ILogger<ItemsController> logger, IItemRepository itemRepository, 
        IUserRepository userRepository,IItemService itemService)
    {
        _logger = logger;
        _itemRepository = itemRepository;
        _userRepository = userRepository;
        _itemService = itemService;
    }


    // GET: api/Items
    [HttpGet]
    public async Task<IActionResult> GetAllItems(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

       
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
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            Item? item = await _itemService.GetAuthorizedItemByIdAsync(id, sender);
            
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Items
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] ItemViewModel item)
    {
        User? sender = await GetUserFromToken();
        
        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();
        
        _logger.LogInformation($"{sender.Name} is sending an item {item.Name}");
        
        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        Item newItem = new Item();

        newItem.BusinessId = sender.BusinessId;
        newItem.Name = item.Name;
        newItem.Description = item.Description;
        newItem.Price = item.Price;
        newItem.BasePrice = item.BasePrice;
        newItem.Category = item.Category;
        newItem.Quantity = item.Quantity;
        
        try
        {
            await _itemRepository.AddItemAsync(newItem);

            return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }


    // PUT: api/Items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemViewModel item)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();
        
        try
        {

            Item? existingItem = await _itemService.GetAuthorizedItemForModificationByIdAsync(id,sender);

            if (existingItem == null)
            {
                return NotFound($"Item with ID {id} not found.");
            }

            existingItem.Price = item.Price;
            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.BasePrice = item.Price;
            existingItem.Category = item.Category;
            existingItem.Quantity = item.Quantity;

            await _itemRepository.UpdateItemAsync(existingItem);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating Item with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // DELETE: api/Items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try
        {
            Item? item = await _itemService.GetAuthorizedItemForModificationByIdAsync(id,sender);
            
            await _itemRepository.DeleteItemAsync(id);
            _logger.LogInformation($"User with id {sender.Id} deleted item with id {item.Id} at {DateTime.Now}");

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating Item with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Items/{id}/Variations
    [HttpGet("{id}/Variations")]
    public async Task<IActionResult> GetAllItemVariations(int id)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();
        try
        {
            Item? item = await _itemService.GetAuthorizedItemByIdAsync(id,sender);
            List<ItemVariation> variations = await _itemService.GetAuthorizedItemVariationsAsync(id, sender);
            
            return Ok(variations);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all variations: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Items/Variations{varId}
    [HttpGet("Variations/{varId}")]
    public async Task<IActionResult> GetItemVariationById(int varId)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try
        {
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving variation with ID {varId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Items/id/Variations
    [HttpPost("{id}/Variations")]
    public async Task<IActionResult> CreateVariation([FromBody] ItemVariation variation)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        var item = await _itemService.GetAuthorizedItemForModificationByIdAsync(variation.ItemId,sender);

        var newVariation = new ItemVariation
        {
            ItemId = item.Id,
            Name = variation.Name,
            AdditionalPrice = variation.AdditionalPrice,
        };

        try
        {
            await _itemRepository.AddItemVariationAsync(newVariation);

            return CreatedAtAction(
                nameof(GetItemVariationById),
                new { id = newVariation.ItemId, varId = newVariation.Id },
                newVariation
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }


    // DELETE: api/Items/Variations/{id}
    [HttpDelete("Variations/{varId}")]
    public async Task<IActionResult> DeleteVariation(int varId)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try
        {
            ItemVariation? variation = await _itemService.GetAuthorizedItemVariationForModificationByIdAsync(varId,sender);
            await _itemRepository.DeleteItemVariationAsync(varId);

            _logger.LogInformation($"Variation with id {varId} deleted at {DateTime.Now} by userId:{sender.Id}");
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting Variation with ID {varId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Items/Variations/{id}
    [HttpPut("Variations/{id}")]
    public async Task<IActionResult> UpdateVariation(int id, VariationsDTO variation)
    {
        try
        {
            User? sender = await GetUserFromToken();
            if (sender == null)
                return Unauthorized();

            ItemVariation? existingVariation = await _itemService.GetAuthorizedItemVariationForModificationByIdAsync(id,sender);

            existingVariation.AdditionalPrice = variation.AdditionalPrice;
            existingVariation.Name = variation.Name;
            existingVariation.ItemId = variation.ItemId;
            existingVariation.Id = variation.Id;


            await _itemRepository.UpdateItemVariationAsync(existingVariation);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Variation with ID {id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating Variation with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    #region HelperMethods
    private async Task<User?> GetUserFromToken()
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization token is missing or null.");
            return null;
        }

        int? userId = Ultilities.ExtractUserIdFromToken(token);
        User? user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning($"Failed to find user with {userId} in DB");
            return null;
        }

        return user;

    }
    #endregion

}
