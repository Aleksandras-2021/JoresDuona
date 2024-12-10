using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using System.Runtime.InteropServices;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ITaxRepository _taxRepository;
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderRepository orderRepository, IUserRepository userRepository, IItemRepository itemRepository,
        ITaxRepository taxRepository, IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _itemRepository = itemRepository;
        _taxRepository = taxRepository;
        _orderService = orderService;
        _logger = logger;
    }


    #region OrderActions
    // GET: api/Order
    [HttpGet("")]
    public async Task<IActionResult> GetAllOrders()
    {

        string? token = HttpContext.Request.Headers["Authorization"].ToString();


        User? senderUser = await GetUserFromToken();
        if (senderUser == null)
        {
            _logger.LogWarning("User not found in DB.");
            return Unauthorized("User not in DB.");
        }
        List<Order> orders;

        if (senderUser.Role == UserRole.SuperAdmin)
            orders = await _orderRepository.GetAllOrdersAsync();
        else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
            orders = await _orderRepository.GetAllBusinessOrdersAsync(senderUser.BusinessId);
        else
            orders = new List<Order>();

        if (orders.Count > 0)
            return Ok(orders);
        else
            return NotFound("No Orders found.");
    }

    // GET: api/Order/id
    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var order = await _orderService.GetAuthorizedOrder(id, sender);

            if (order == null)
                return NotFound($"Order with ID {id} not found.");

            await RecalculateOrderCharge(id);

            return Ok(order);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving order with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        User? sender = await GetUserFromToken();

        _logger.LogInformation($"User with id: {sender.Id} is creating an order at {DateTime.Now}");

        if (sender == null)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        Order newOrder = new Order();

        newOrder.BusinessId = sender.BusinessId;
        newOrder.CreatedAt = DateTime.UtcNow.AddHours(2); ;
        newOrder.ClosedAt = null;
        newOrder.UserId = sender.Id;
        newOrder.Status = OrderStatus.Open;
        newOrder.ChargeAmount = 0;
        newOrder.DiscountAmount = 0;
        newOrder.TaxAmount = 0;
        newOrder.TipAmount = 0;
        newOrder.Payments = new List<Payment>();
        newOrder.OrderDiscounts = new List<OrderDiscount>();


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
    //Order/{id}/UpdateStatus
    [HttpPost("{orderId}/UpdateStatus/{status}")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int orderId, OrderStatus status)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        _logger.LogInformation($"User with id: {sender.Id} is updating an order at {DateTime.Now}, orderId:{orderId}");

        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound($"Order with ID {orderId} not found.");
            if (order.Status == OrderStatus.Closed || sender.BusinessId != order.BusinessId)
                return Unauthorized("You are not authorized to modify closed order.");

            // Update the status
            order.Status = status;

            if (order.Status == OrderStatus.Closed)
                order.ClosedAt = DateTime.UtcNow.AddHours(2);

            await _orderRepository.UpdateOrderAsync(order);

            return Ok(new { message = "Order status updated successfully.", status });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating order status: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }


    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrder([FromBody] Order order)
    {
        if (order == null)
        {
            return BadRequest("Invalid order data.");
        }

        if (order.Status == OrderStatus.Closed)
        {
            return Unauthorized("Cannot modify closed order.");
        }

        try
        {
            User? sender = await GetUserFromToken();

            Order? existingOrder = await _orderRepository.GetOrderByIdAsync(order.Id);

            if (existingOrder == null)
            {
                return NotFound($"Order with ID {order.Id} not found.");
            }
            if (sender == null || order.Status == OrderStatus.Closed || order.BusinessId != sender.BusinessId)
                return Unauthorized();

            //do not update created at
            //DO not update businessID
            //update closed at if new status is closed
            //Recalculate ChargeAmount
            //Recalculate Tax Amount
            //

            existingOrder.UserId = sender.Id; //Whoever updates order, takes over the ownership of it
            existingOrder.User = sender;
            existingOrder.Status = order.Status;
            existingOrder.ChargeAmount = order.ChargeAmount;
            existingOrder.DiscountAmount = order.DiscountAmount;
            existingOrder.TaxAmount = order.TaxAmount;
            existingOrder.TipAmount = order.TipAmount;

            await _orderRepository.UpdateOrderAsync(existingOrder);
            // await RecalculateOrderCharge(existingOrder.Id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Order with ID {order.Id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating Item with ID {order.Id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
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
        if (order.Status == OrderStatus.Closed)
            return Unauthorized("You are not authorized to modify closed order.");

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
    #endregion


    #region HelperMethods
    private async Task RecalculateOrderCharge(int orderId)
    {
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order.Status == OrderStatus.Closed)
            return;

        List<OrderItem> orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);
        List<OrderItemVariation> orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderIdAsync(orderId);
        Tax tax;

        order.ChargeAmount = 0;
        order.TaxAmount = 0;

        //base charge(untaxed)
        foreach (var item in orderItems)
        {
            order.ChargeAmount += item.Price * item.Quantity;


            tax = await _taxRepository.GetTaxByItemIdAsync(item.ItemId);

            if (tax.IsPercentage)
                order.TaxAmount += item.Price * item.Quantity * tax.Amount / 100;
            else
                order.TaxAmount += tax.Amount;
        }

        foreach (var variation in orderItemVariations)
        {
            order.ChargeAmount += variation.AdditionalPrice * variation.Quantity;
            OrderItem orderItemForVar = await _orderRepository.GetOrderItemById(variation.OrderItemId);

            tax = await _taxRepository.GetTaxByItemIdAsync(orderItemForVar.ItemId);

            if (tax.IsPercentage)
                order.TaxAmount += variation.AdditionalPrice * variation.Quantity * tax.Amount / 100;
            else
                order.TaxAmount += tax.Amount;
        }
        await _orderRepository.UpdateOrderAsync(order);
    }

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
