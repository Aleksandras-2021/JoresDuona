using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Repositories;
using PosAPI.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosAPI.Services.Interfaces;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/Order")]//Documentation says so
[ApiController]
public class OrderItemsController : ControllerBase
{
    private readonly IUserTokenService _userTokenService;
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderItemsController> _logger;

    public OrderItemsController(IUserTokenService userTokenService, IOrderService orderService, ILogger<OrderItemsController> logger)
    {
        _userTokenService = userTokenService;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("{orderId}/Items")]
    public async Task<IActionResult> GetOrderItems(int orderId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var orderItems = await _orderService.GetAuthorizedOrderItems(orderId, sender);
        return Ok(orderItems);
    }

    // GET: api/Order/{OrderId}/Items/{id}
    [HttpGet("Items/{id}")]
    public async Task<IActionResult> GetOrderItem(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var orderItem = await _orderService.GetAuthorizedOrderItem(id, sender);
        return Ok(orderItem);
    }

    // POST: api/Order/{orderId}/Items
    [HttpPost("{orderId}/Items")]
    public async Task<IActionResult> AddItemToOrder(int orderId, [FromBody] AddItemDTO addItemDTO)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var orderItem = await _orderService.CreateAuthorizedOrderItem(orderId, addItemDTO, sender);
        return CreatedAtRoute("GetOrderById", new { id = orderId }, orderItem);
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}
    [HttpDelete("{orderId}/Items/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _orderService.DeleteAuthorizedOrderItem(orderId,orderItemId,sender);
        return Ok("Order item deleted successfully.");
    }
}
