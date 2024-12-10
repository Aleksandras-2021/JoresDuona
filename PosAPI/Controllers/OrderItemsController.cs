using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;

namespace PosAPI.Controllersq;


[Authorize]
[Route("api/Order")]//Documentation says so
[ApiController]
public class OrderItemsController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderItemsController> _logger;

    public OrderItemsController(IOrderRepository orderRepository, IUserRepository userRepository, IOrderService orderService, ILogger<OrderItemsController> logger, IItemRepository itemRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderService = orderService;
        _itemRepository = itemRepository;

        _logger = logger;
    }

    [HttpGet("{orderId}/Items")]
    public async Task<IActionResult> GetOrderItems(int orderId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var orderItems = await _orderService.GetAuthorizedOrderItems(orderId, sender);

            if (orderItems == null || !orderItems.Any())
                return NotFound("No items found for this order.");

            return Ok(orderItems);
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
            _logger.LogError($"Error retrieving items for order ID {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    // GET: api/Order/{OrderId}/Items/{id}
    [HttpGet("Items/{id}")]
    public async Task<IActionResult> GetOrderItem(int id)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var orderItem = await _orderService.GetAuthorizedOrderItem(id, sender);

            return Ok(orderItem);
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
            _logger.LogError($"Error retrieving order item with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    // POST: api/Order/{orderId}/Items
    [HttpPost("{orderId}/Items")]
    public async Task<IActionResult> AddItemToOrder(int orderId, [FromBody] AddItemDTO addItemDTO)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var order = await _orderService.GetAuthorizedOrderForModification(orderId, sender);

            var item = await _itemRepository.GetItemByIdAsync(addItemDTO.ItemId);

            OrderItem orderItem = new OrderItem
            {
                OrderId = orderId,
                ItemId = addItemDTO.ItemId,
                Quantity = 1, //or addItemDto.Quantity if that ever gets implemented
                Price = item.Price
            };

            await _orderRepository.AddOrderItemAsync(orderItem);
            await _orderRepository.UpdateOrderAsync(order);
            await _orderService.RecalculateOrderCharge(orderItem.OrderId);

            return CreatedAtRoute("GetOrderById", new { id = order.Id }, orderItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}
    [HttpDelete("{orderId}/Items/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            // Get Order for validation on its status
            var order = await _orderService.GetAuthorizedOrderForModification(orderId, sender);
            // Validate order item
            var orderItem = await _orderService.GetAuthorizedOrderItem(orderItemId, sender);

            // Delete the order item
            await _orderRepository.DeleteOrderItemAsync(orderItemId);

            // Recalculate charges
            await _orderService.RecalculateOrderCharge(orderItem.OrderId);

            return Ok("Order item deleted successfully.");
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
            _logger.LogError($"Error deleting order item with ID {orderItemId}: {ex.Message}");
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
