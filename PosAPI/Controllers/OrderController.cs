using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Repositories;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<OrderController> _logger;
    public OrderController(IOrderRepository orderRepository, IUserRepository userRepository, IItemRepository itemRepository, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _itemRepository = itemRepository;
        _logger = logger;
    }

    // GET: api/Order
    [HttpGet("")]
    public async Task<IActionResult> GetAllOrders()
    {

        string? token = HttpContext.Request.Headers["Authorization"].ToString();
        _logger.LogInformation(token);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization token is missing or null.");
            return Unauthorized("Authorization token is missing.");
        }

        int? userId = Ultilities.ExtractUserIdFromToken(token);

        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found in DB.");
            return Unauthorized("User not in DB.");
        }
        List<Order> orders;

        if (user.Role == UserRole.SuperAdmin)
            orders = await _orderRepository.GetAllOrdersAsync();
        else if (user.Role == UserRole.Manager || user.Role == UserRole.Owner || user.Role == UserRole.Worker)
            orders = await _orderRepository.GetAllBusinessOrdersAsync(user.BusinessId);
        else
            orders = new List<Order>();

        if (orders.Count > 0)
            return Ok(orders);
        else
            return NotFound("No Orders found.");
    }

    // GET: api/Order/id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        User? senderUser = await GetUserFromToken();

        if (senderUser == null)
            return Unauthorized();

        try
        {
            Order? order;

            if (senderUser.Role == UserRole.SuperAdmin)
            {
                order = await _orderRepository.GetOrderByIdAsync(id);
            }
            else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
            {
                order = await _orderRepository.GetOrderByIdAsync(id);

                if (order.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        User? sender = await GetUserFromToken();

        _logger.LogInformation($"User with id: {sender.Id} is creating an order at {DateTime.Now}");

        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        Order newOrder = new Order();

        newOrder.BusinessId = sender.BusinessId;
        newOrder.CreatedAt = DateTime.UtcNow;
        newOrder.ClosedAt = null;
        newOrder.UserId = sender.Id;
        newOrder.Status = OrderStatus.Open;
        newOrder.ChargeAmount = 0;
        newOrder.DiscountAmount = 0;
        newOrder.TaxAmount = 0;
        newOrder.TipAmount = 0;


        try
        {
            await _orderRepository.AddOrderAsync(newOrder);

            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    // POST: api/Order/{orderId}/AddItem
    [HttpPost("{orderId}/AddItem")]
    public async Task<IActionResult> AddItemToOrder(int orderId, [FromBody] AddItemDTO addItemDTO)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        // Check if the order exists
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound($"Order with ID {orderId} not found.");

        if (order.BusinessId != sender.BusinessId)
            return Unauthorized("You are not authorized to modify this order.");

        Item item = await _itemRepository.GetItemByIdAsync(addItemDTO.ItemId);

        // Create the OrderItem
        OrderItem orderItem = new OrderItem
        {
            OrderId = orderId,
            ItemId = addItemDTO.ItemId,
            Quantity = 1,
            Price = item.Price
        };

        try
        {
            // Add the OrderItem to the database
            await _orderRepository.AddOrderItemAsync(orderItem);

            order.ChargeAmount += orderItem.Price * orderItem.Quantity;
            await _orderRepository.UpdateOrderAsync(order);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderItem);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
        {
            return Unauthorized();
        }

        // Check if the order exists
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound($"Order with ID {orderId} not found.");
        }

        if (order.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
        {
            return Unauthorized("You are not authorized to delete this order.");
        }

        try
        {
            // Delete the order and its associated data
            await _orderRepository.DeleteOrderAsync(orderId);

            return Ok("Order deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting order with ID {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }


    [HttpGet("{orderId}/OrderItems")]
    public async Task<IActionResult> GetOrderItems(int orderId)
    {
        var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        if (orderItems == null || !orderItems.Any())
        {
            return NotFound("No items found for this order.");
        }

        return Ok(orderItems);
    }

    // DELETE: api/Order/{orderId}/DeleteItem/{orderItemId}
    [HttpDelete("{orderId}/DeleteItem/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        // Verify the order exists and is associated with the user's business
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound($"Order with ID {orderId} not found.");

        if (order.BusinessId != sender.BusinessId)
            return Unauthorized("You are not authorized to modify this order.");

        try
        {
            // Delete the OrderItem
            await _orderRepository.DeleteOrderItemAsync(orderItemId);

            // Update the order's charge amount
            var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);
            order.ChargeAmount = orderItems.Sum(oi => oi.Price * oi.Quantity);

            await _orderRepository.UpdateOrderAsync(order);

            return NoContent();
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
