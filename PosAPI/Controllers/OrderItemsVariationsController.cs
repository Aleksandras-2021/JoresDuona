using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/Order")] //Tech document specifies this
[ApiController]
public class OrderItemsVariationsController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<OrderItemsVariationsController> _logger;

    public OrderItemsVariationsController(IUserRepository userRepository, IOrderService orderService, ILogger<OrderItemsVariationsController> logger)
    {
        _userRepository = userRepository;
        _orderService = orderService;
        _logger = logger;
    }


    [HttpGet("{orderId}/Items/{orderItemId}/Variations")]
    public async Task<IActionResult> GetOrderItemVariations(int orderItemId)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var orderItemVariations = await _orderService.GetAuthorizedOrderItemVariations(orderItemId, sender);

            return Ok(orderItemVariations);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"{ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving order item variations: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    // GET: api/Order/{orderId}/Items/{orderItemId}/Variations/{variationId}
    [HttpGet("{orderId}/Items/{orderItemId}/Variations/{variationId}")]
    public async Task<IActionResult> GetOrderItemVariationById(int orderId, int orderItemId, int variationId)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var orderItemVariation = await _orderService.GetAuthorizedOrderItemVariation(variationId, orderItemId, sender);

            return Ok(orderItemVariation);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"{ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving variation with ID {variationId} for OrderItem {orderItemId} in Order {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("{orderId}/Items/{itemId}/Variations")]
    public async Task<IActionResult> AddOrderItemVariation(int orderId, int itemId, [FromBody] AddVariationDTO addVariationDTO)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var orderItemVariation =
                await _orderService.CreateAuthorizedOrderItemVariation(orderId, itemId, addVariationDTO, sender);
            
            
            return CreatedAtAction(nameof(GetOrderItemVariationById),
                new { orderId = orderId, orderItemId = itemId, variationId = orderItemVariation.Id },
                orderItemVariation);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"{ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding order item variation: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}
    [HttpDelete("{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}")]
    public async Task<IActionResult> DeleteOrderItemVariation(int orderId, int orderItemId, int orderItemVariationId)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            await _orderService.DeleteAuthorizedOrderItemVariation(orderId, orderItemId, orderItemVariationId, sender);

            return Ok("Order Item Variation deleted successfully.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"{ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting order variation with ID {orderItemVariationId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
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
