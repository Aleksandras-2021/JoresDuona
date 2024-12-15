using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using PosAPI.Repositories;
using PosAPI.Services;
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
    private readonly IUserRepository _userRepository;
    private readonly IItemService _itemService;
    public ItemsController(ILogger<ItemsController> logger, IItemRepository itemRepository, 
        IUserRepository userRepository,IItemService itemService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _itemService = itemService;
    }
    
    // GET: api/Items
    [HttpGet]
    public async Task<IActionResult> GetAllItems(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();

        try
        {
            var paginatedItems = await _itemService.GetAuthorizedItemsAsync(sender, pageNumber, pageSize);

            if (paginatedItems.Items.Count > 0)
                return Ok(paginatedItems);
            else
                return NotFound("No Items found.");
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
            _logger.LogError($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        User? sender = await GetUserFromToken();

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
            _logger.LogError($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Items
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] ItemViewModel item)
    {
        User? sender = await GetUserFromToken();

        try
        {
            var newItem = await _itemService.CreateAuthorizedItemAsync(item,sender);
            
            _logger.LogInformation($"{sender.Name} is creating an item {item.Name}");


            return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
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
            _logger.LogError($"Internal server Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    // PUT: api/Items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemViewModel item)
    {
        User? sender = await GetUserFromToken();
        try
        {
            await _itemService.UpdateAuthorizedItemAsync(id,item, sender);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"{ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"{ex.Message}");
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

        try
        {
            await _itemService.DeleteAuthorizedItemAsync(id,sender);
            _logger.LogInformation($"User with id {sender.Id} deleted item with id {id} at {DateTime.Now}");

            return Ok();
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

        try
        {
            List<ItemVariation> variations = await _itemService.GetAuthorizedItemVariationsAsync(id, sender);
            
            return Ok(variations);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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
        
        try
        {
            var newVariation = await _itemService.CreateAuthorizedItemVariationAsync(variation, sender);

            return CreatedAtAction(
                nameof(GetItemVariationById),
                new { id = newVariation.Id, varId = newVariation.Id },
                newVariation
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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

        try
        {
            await _itemService.DeleteAuthorizedItemVariationAsync(varId, sender);

            _logger.LogInformation($"Variation with id {varId} deleted at {DateTime.Now} by userId:{sender.Id}");
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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
        User? sender = await GetUserFromToken();
        try
        {

            await _itemService.UpdateAuthorizedItemVariationAsync(id,variation,sender);

            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Variation with ID {id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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

        if(user == null)
        {
            _logger.LogWarning($"Failed to find user with {userId} in DB");
            return null;
        }

        return user;

    }
    #endregion

}
