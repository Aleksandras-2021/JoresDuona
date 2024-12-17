using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Repositories.Interfaces;
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
    private readonly IDiscountRepository _discountRepository;
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderController> _logger;


    public OrderController(IOrderRepository orderRepository, IUserRepository userRepository, IOrderService orderService, ILogger<OrderController> logger, IDiscountRepository discountRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderService = orderService;
        _logger = logger;
        _discountRepository = discountRepository;
    }

    // GET: api/Order
    [HttpGet("")]
    public async Task<IActionResult> GetAllOrders(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();

        try
        {
            var paginatedOrders = await _orderService.GetAuthorizedOrders(sender, pageNumber, pageSize);

            if (paginatedOrders.Items.Count > 0)
                return Ok(paginatedOrders);
            else
                return NotFound("No Orders found.");
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
            _logger.LogError($"Error retrieving orders: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    // GET: api/Order/id
    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            Order? order = await _orderService.GetAuthorizedOrder(id, sender);

            //await _orderService.RecalculateOrderCharge(id);

            return Ok(order);
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
            _logger.LogError($"Error retrieving order with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            int orderId = await _orderService.CreateAuthorizedOrder(sender);

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { Id = orderId });
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
            _logger.LogError($"Error creating order: {ex.Message}");
            return StatusCode(500, "Internal server error.");
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

      
            order.Status = status;

            if (order.Status == OrderStatus.Closed)
                order.ClosedAt = DateTime.UtcNow.AddHours(2);

            await _orderRepository.UpdateOrderAsync(order);

            return Ok(new { message = "Order status updated successfully.", status });
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
        User? sender = await GetUserFromToken();

        try
        {

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
            _logger.LogWarning($"403 Status, User {sender.Id}. {ex.Message}");
            return StatusCode(403, $"Forbidden {ex.Message}");
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
            var orderVariations = await _orderService.GetAuthorizedOrderVariations(orderId, sender);

            return Ok(orderVariations);
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
    
            [HttpGet("{orderId}/Services")]
    public async Task<IActionResult> GetAllOrderServices(int orderId)
    {
        User? sender = await GetUserFromToken();

        try
        {
            var orderServices = await _orderService.GetAuthorizedOrderServices(orderId, sender);

            return Ok(orderServices);
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

    [HttpPut("{orderId}/apply-discount")]
    public async Task<IActionResult> ApplyDiscountToOrder(int orderId, [FromForm] string discountName)
    {
        // 1. Fetch the discount by name
        var discount = await _discountRepository.GetActiveDiscountByNameAsync(discountName);
        if (discount == null)
        {
            return NotFound(new { Message = "Discount not found or not active." });
        }

        // 2. Fetch the order using the provided order ID
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound(new { Message = "Order not found." });
        }

        // 3. Calculate the total amount of the order (including charges, tax, and tip)
        var totalAmount = order.ChargeAmount + order.TaxAmount + order.TipAmount - order.DiscountAmount;

        // 4. Apply the discount based on whether it's a percentage or fixed amount
        decimal discountApplied = 0;
        if (discount.IsPercentage)
        {
            // Apply percentage-based discount
            discountApplied = totalAmount * (discount.Amount / 100);
        }
        else
        {
            // Apply fixed amount discount
            discountApplied = discount.Amount;
        }

        // 5. Update the order's discount and charge amounts
        order.DiscountAmount += discountApplied;
        order.ChargeAmount = totalAmount - order.DiscountAmount; 
        order.DiscountId = discount.Id; 

        try
        {
            // Save the updated order
            await _orderRepository.UpdateOrderAsync(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error applying discount: {ex.Message}" });
        }

        // 6. Return the updated order with the savings message
        var savingsText = discount.IsPercentage
            ? $"{discount.Amount}%"
            : $"{discount.Amount:C}";

        return Ok(new
        {
            Message = $"You saved {savingsText}. Total after discount: {order.ChargeAmount:C}.",
            Order = new
            {
                order.Id,
                order.ChargeAmount,
                order.DiscountAmount,
                order.TaxAmount,
                order.TipAmount
            }
        });
    }





}
