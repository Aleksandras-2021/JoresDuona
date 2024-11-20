using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosAPI.Migrations;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace PosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly IItemRepository _itemRepository;
        private readonly IUserRepository _userRepository;
        public ItemsController(ILogger<ItemsController> logger, IItemRepository itemRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _itemRepository = itemRepository;
            _userRepository = userRepository;
        }


        // GET: api/Items
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            User? sender = await GetUserFromToken();

            if (sender == null)
                return Unauthorized();

            try
            {
                List<Item> items;
                if (sender.Role == UserRole.SuperAdmin)
                {
                    items = await _itemRepository.GetAllItemsAsync();
                }
                else
                {
                    items = await _itemRepository.GetAllBusinessItemsAsync(sender.BusinessId);
                }


                if (items == null || items.Count == 0)
                {
                    return NotFound("No items found.");
                }


                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all items: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Items
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemViewModel item)
        {
            User? sender = await GetUserFromToken();

            _logger.LogInformation($"{sender.Name} is sending an item {item.Name}");

            if (item == null)
                return BadRequest("Item data is null.");

            if (sender == null)
                return Unauthorized();

            if (sender.BusinessId <= 0)
                return BadRequest("Invalid BusinessId associated with the user.");

            Item newItem = new Item();

            newItem.BusinessId = sender.BusinessId;
            newItem.Name = item.Name;
            newItem.Description = item.Description;
            newItem.Price = item.Price;
            newItem.BasePrice = item.BasePrice;
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
            if (item == null)
            {
                return BadRequest("Invalid item data.");
            }

            try
            {
                User? sender = await GetUserFromToken();

                Item? existingItem = await _itemRepository.GetItemByIdAsync(id);

                if (existingItem == null)
                {
                    return NotFound($"Item with ID {id} not found.");
                }
                if (sender == null || sender.Role == UserRole.Worker)
                    return Unauthorized();

                existingItem.Price = item.Price;
                existingItem.Name = item.Name;
                existingItem.Description = item.Description;
                existingItem.BasePrice = item.Price;
                existingItem.Quantity = item.Quantity;


                await _itemRepository.UpdateItemAsync(existingItem);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Item with ID {id} not found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating Item with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: api/Items/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null)
                return Unauthorized();

            try
            {
                Item? item;

                if (senderUser.Role == UserRole.SuperAdmin)
                {
                    item = await _itemRepository.GetItemByIdAsync(id);
                }
                else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner)
                {
                    item = await _itemRepository.GetItemByIdAsync(id);

                    if (item.BusinessId != senderUser.BusinessId)
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }

                if (item == null)
                {
                    return NotFound($"Item with ID {id} not found.");
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        // DELETE: api/Items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            User? sender = await GetUserFromToken();

            if (sender == null || sender.Role == UserRole.Worker)
                return Unauthorized();

            try
            {
                Item? item = await _itemRepository.GetItemByIdAsync(id);

                if (item == null)
                {
                    return NotFound($"Item with ID {id} not found.");
                }

                if (sender.Role == UserRole.SuperAdmin)
                {
                    await _itemRepository.DeleteItemAsync(id);
                }
                else if ((sender.Role == UserRole.Owner || sender.Role == UserRole.Manager) && item.BusinessId == sender.BusinessId)
                {
                    await _itemRepository.DeleteItemAsync(id);
                }
                else
                {
                    return Unauthorized();
                }

                _logger.LogInformation($"User with id {sender.Id} deleted item with id {item.Id} at {DateTime.Now}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting item with ID {id}: {ex.Message}");
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
                List<ItemVariation> variations = await _itemRepository.GetItemVariationsAsync(id);

                if (variations == null || variations.Count == 0)
                {
                    return NotFound("No variations found.");
                }

                return Ok(variations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all variations: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Items/{id}/Variations{varId}
        [HttpGet("{id}/Variations/{varId}")]
        public async Task<IActionResult> GetItemVariationById(int varId)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null)
                return Unauthorized();

            try
            {
                ItemVariation variation = await _itemRepository.GetItemVariationByIdAsync(varId);

                if (variation == null)
                {
                    return NotFound("No variation found.");
                }

                if (variation.Item.BusinessId != senderUser.BusinessId && senderUser.Role != UserRole.SuperAdmin)
                {
                    return Unauthorized();
                }

                return Ok(variation);
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
            if (variation == null)
                return BadRequest("Variation data is null.");

            User? sender = await GetUserFromToken();

            var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
            if (item == null)
                return NotFound($"Item with ID {variation.ItemId} not found.\n");

            if (sender == null || (item.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin))
                return Unauthorized();

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
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
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
}
