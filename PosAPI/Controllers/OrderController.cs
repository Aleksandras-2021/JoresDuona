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
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderRepository orderRepository, IUserRepository userRepository, IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderService = orderService;
        _logger = logger;
    }

    // GET: api/Order
    [HttpGet("")]
    public async Task<IActionResult> GetAllOrders()
    {
        string? token = HttpContext.Request.Headers["Authorization"].ToString();

        User? senderUser = await GetUserFromToken();
        if (senderUser == null)
        {
            return Unauthorized();
        }
        List<Order> orders = await _orderService.GetAuthorizedOrders(senderUser);

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
            Order? order = await _orderService.GetAuthorizedOrder(id, sender);

            await _orderService.RecalculateOrderCharge(id);

            return Ok(order);
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
    // /api/Order/{id}/UpdateStatus
    [HttpPost("{orderId}/UpdateStatus/{status}")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int orderId, OrderStatus status)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        _logger.LogInformation($"User with id: {sender.Id} is updating an order at {DateTime.Now}, orderId:{orderId}");

        try
        {
            Order? order = await _orderService.GetAuthorizedOrder(orderId, sender);

            // Update the status
            order.Status = status;

            if (order.Status == OrderStatus.Closed)
                order.ClosedAt = DateTime.UtcNow.AddHours(2);

            await _orderRepository.UpdateOrderAsync(order);

            return Ok(new { message = "Order status updated successfully.", status });
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

            if (sender == null)
                return Unauthorized();

            Order? existingOrder = await _orderService.GetAuthorizedOrderForModification(order.Id, sender);

            existingOrder.UserId = sender.Id; //Whoever updates order, takes over the ownership of it
            existingOrder.User = sender;
            existingOrder.Status = order.Status;
            existingOrder.ChargeAmount = order.ChargeAmount;
            existingOrder.DiscountAmount = order.DiscountAmount;
            existingOrder.TaxAmount = order.TaxAmount;
            existingOrder.TipAmount = order.TipAmount;

            await _orderRepository.UpdateOrderAsync(existingOrder);

            return NoContent();
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

        try
        {
            Order? order = await _orderService.GetAuthorizedOrderForModification(orderId, sender);

            await _orderRepository.DeleteOrderAsync(orderId);

            return Ok("Order deleted successfully.");
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
            _logger.LogError($"Error deleting order with ID {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{orderId}/Variations")]
    public async Task<IActionResult> GetAllOrderVariations(int orderId)
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            var OrderVariations = await _orderService.GetAuthorizedOrderVariations(orderId, sender);

            return Ok(OrderVariations);
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
