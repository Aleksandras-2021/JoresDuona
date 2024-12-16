using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.Models;
using PosShared.Utilities;
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
    public async Task<IActionResult> GetAllOrders(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();

        var paginatedOrders = await _orderService.GetAuthorizedOrders(sender, pageNumber, pageSize);

        if (paginatedOrders.Items.Count > 0)
            return Ok(paginatedOrders);
        else
            return NotFound("No Orders found.");
    }


    // GET: api/Order/id
    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        User? sender = await GetUserFromToken();
        Order? order = await _orderService.GetAuthorizedOrder(id, sender);
        return Ok(order);
    }

    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        User? sender = await GetUserFromToken();
        int orderId = await _orderService.CreateAuthorizedOrder(sender);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { Id = orderId });
    }
    // /api/Order/{id}/UpdateStatus
    [HttpPost("{orderId}/UpdateStatus/{status}")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int orderId, OrderStatus status)
    {
        User? sender = await GetUserFromToken();
        
        _logger.LogInformation($"User with id: {sender.Id} is updating an order at {DateTime.Now}, orderId:{orderId}");
        
        Order? order = await _orderService.GetAuthorizedOrder(orderId, sender);

        order.Status = status;

        if (order.Status == OrderStatus.Closed)
            order.ClosedAt = DateTime.UtcNow.AddHours(2);
            
        await _orderRepository.UpdateOrderAsync(order);

        return Ok(new { message = "Order status updated successfully.", status });
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
            return Forbid("Cannot modify closed order.");
        }
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

        return Ok();
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
        var orderVariations = await _orderService.GetAuthorizedOrderVariations(orderId, sender);
        return Ok(orderVariations);
    }
    
            [HttpGet("{orderId}/Services")]
    public async Task<IActionResult> GetAllOrderServices(int orderId)
    {
        User? sender = await GetUserFromToken();
        var orderServices = await _orderService.GetAuthorizedOrderServices(orderId, sender);
        return Ok(orderServices);
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
