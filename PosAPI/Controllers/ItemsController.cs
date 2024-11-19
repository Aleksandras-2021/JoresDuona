using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosAPI.Migrations;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;

namespace PosAPI.Controllers
{
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
                _logger.LogError($"Error retrieving all users: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Items
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            User? sender = await GetUserFromToken();

            if (sender == null || sender.Role == UserRole.Worker)
                return Unauthorized();

            if (item == null)
                return BadRequest("Item data is null.");


            Item newItem = new Item();

            newItem.Name = item.Name;
            newItem.Description = item.Description;
            newItem.ImageUrl = item.ImageUrl;
            newItem.BasePrice = item.BasePrice;
            newItem.Price = item.Price;
            newItem.Quantity = item.Quantity;

            if (sender.Role == UserRole.SuperAdmin) //Only admins can set item business ID
            {
                newItem.BusinessId = item.BusinessId;
            }
            else //Business owners/Managers can only create items for their business
            {
                newItem.BusinessId = sender.BusinessId;
            }

            try
            {
                await _itemRepository.UpdateItemAsync(newItem);
                return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Items/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null)
                return BadRequest();

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
