using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Controllersq;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/Order")] //Tech document specifies this
[ApiController]
public class OrderItemsVariationsController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<OrderItemsVariationsController> _logger;

    public OrderItemsVariationsController(IUserRepository userRepository, IOrderService orderService, IOrderRepository orderRepository, ILogger<OrderItemsVariationsController> logger)
    {
        _userRepository = userRepository;
        _orderService = orderService;
        _orderRepository = orderRepository;
        _logger = logger;
    }


    [HttpGet("{orderId}/Items/{orderItemId}/ItemVariations")]
    public async Task<IActionResult> GetItemVariations(int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var orderItem = await _orderService.GetAuthorizedOrderItem(orderItemId, sender);

            var itemVariations = await _orderService.GetAuthorizedItemVariations(orderItemId, sender);

            return Ok(itemVariations);
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
            _logger.LogError($"Error retrieving item variations: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }



    [HttpGet("{orderId}/Items/{orderItemId}/Variations")]
    public async Task<IActionResult> GetOrderItemVariations(int orderItemId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            // Step 1: Validate Order Item and Get Variations
            var orderItem = await _orderService.GetAuthorizedOrderItem(orderItemId, sender);
            var orderItemVariations = await _orderService.GetAuthorizedOrderItemVariations(orderItemId, sender);

            // Step 2: Return Order Item Variations
            return Ok(orderItemVariations);
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
            _logger.LogError($"Error retrieving order item variations: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }





    // GET: api/Order/{orderId}/Items/{orderItemId}/Variations/{variationId}
    [HttpGet("{orderId}/Items/{orderItemId}/Variations/{variationId}")]
    public async Task<IActionResult> GetOrderItemVariationById(int orderId, int orderItemId, int variationId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var orderItemVariation = await _orderService.GetAuthorizedOrderItemVariation(variationId, orderItemId, sender);

            return Ok(orderItemVariation);
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
            _logger.LogError($"Error retrieving variation with ID {variationId} for OrderItem {orderItemId} in Order {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("{orderId}/Items/{itemId}/Variations")]
    public async Task<IActionResult> AddOrderItemVariation(int orderId, int itemId, [FromBody] AddVariationDTO addVariationDTO)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            // Step 1: Validate Order, OrderItem, and Variation
            var order = await _orderService.GetAuthorizedOrder(orderId, sender);
            var orderItem = await _orderService.GetAuthorizedOrderItem(itemId, sender);
            var variation = await _orderService.GetAuthorizedItemVariation(addVariationDTO.VariationId, sender);

            // Step 2: Create and Add Order Item Variation
            var orderItemVariation = new OrderItemVariation
            {
                OrderItemId = itemId,
                ItemVariationId = addVariationDTO.VariationId,
                Quantity = addVariationDTO.Quantity,
                AdditionalPrice = variation.AdditionalPrice,
                ItemVariation = variation,
                OrderItem = orderItem
            };

            await _orderRepository.AddOrderItemVariationAsync(orderItemVariation);
            await _orderRepository.UpdateOrderAsync(order);

            // Step 3: Return the Created Resource
            return CreatedAtAction(nameof(GetOrderItemVariationById),
                new { orderId = orderId, orderItemId = itemId, variationId = orderItemVariation.Id },
                orderItemVariation);
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
            _logger.LogError($"Error adding order item variation: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}
    [HttpDelete("{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}")]
    public async Task<IActionResult> DeleteOrderItemVariation(int orderId, int orderItemId, int orderItemVariationId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        _logger.LogWarning($"{sender.Id} tries to delete Order item variation :" +
            $"\nOrderID: {orderId}, OrderItemId:{orderItemId}, OrderItemVariationId:{orderItemVariationId}");

        try
        {
            var order = await _orderService.GetAuthorizedOrder(orderId, sender);
            var variation = await _orderService.GetAuthorizedOrderItemVariation(orderItemVariationId, orderItemId, sender);

            // Step 2: Delete the OrderItemVariation
            await _orderRepository.DeleteOrderItemVariationAsync(orderItemVariationId);

            // Step 3: Return Success
            return Ok("Order Item Variation deleted successfully.");
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
