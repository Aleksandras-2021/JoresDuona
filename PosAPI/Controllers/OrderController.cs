using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.Models;
using PosShared.Utilities;
namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserTokenService _userTokenService;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderRepository orderRepository, IUserTokenService userTokenService, IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userTokenService = userTokenService;
        _orderService = orderService;
        _logger = logger;
    }

    // GET: api/Order
    [HttpGet("")]
    public async Task<IActionResult> GetAllOrders(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
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
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Order? order = await _orderService.GetAuthorizedOrder(id, sender);
        return Ok(order);
    }

    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var order = await _orderService.CreateAuthorizedOrder(sender);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { Id = order.Id });
    }
    // /api/Order/{id}/UpdateStatus
    [HttpPost("{orderId}/UpdateStatus/{status}")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int orderId, OrderStatus status)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Order? order = await _orderService.GetAuthorizedOrder(orderId, sender);

        order.Status = status;

        if (order.Status == OrderStatus.Closed)
            order.ClosedAt = DateTime.UtcNow.AddHours(2);

        await _orderRepository.UpdateOrderAsync(order);

        return Ok(new { message = "Order status updated successfully.", status });
    }


    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrder([FromBody] Order? order)
    {
        if (order == null)
        {
            return BadRequest("Invalid order data.");
        }

        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _orderService.UpdateAuthorizedOrder(order, sender);

        return Ok("Order updated");
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _orderService.DeleteAuthorizedOrder(orderId, sender);
        return Ok("Order deleted successfully.");
    }

    [HttpGet("{orderId}/Variations")]
    public async Task<IActionResult> GetAllOrderVariations(int orderId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var orderVariations = await _orderService.GetAuthorizedOrderVariations(orderId, sender);
        return Ok(orderVariations);
    }

    [HttpGet("{orderId}/Services")]
    public async Task<IActionResult> GetAllOrderServices(int orderId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var orderServices = await _orderService.GetAuthorizedOrderServices(orderId, sender);
        return Ok(orderServices);
    }
}