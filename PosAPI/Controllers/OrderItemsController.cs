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
    private readonly IUserRepository _userRepository;
    private readonly ITaxService _taxService;
    private readonly IOrderService _orderService;

    private readonly ILogger<OrderItemsController> _logger;

    public OrderItemsController(IUserRepository userRepository, IOrderService orderService, ILogger<OrderItemsController> logger, ITaxService taxService)
    {
        _userRepository = userRepository;
        _orderService = orderService;
        _taxService = taxService;

        _logger = logger;
    }

    [HttpGet("{orderId}/Items")]
    public async Task<IActionResult> GetOrderItems(int orderId)
    {
        User? sender = await GetUserFromToken();
        var orderItems = await _orderService.GetAuthorizedOrderItems(orderId, sender);
        return Ok(orderItems);
    }

    // GET: api/Order/{OrderId}/Items/{id}
    [HttpGet("Items/{id}")]
    public async Task<IActionResult> GetOrderItem(int id)
    {
        User? sender = await GetUserFromToken();
        var orderItem = await _orderService.GetAuthorizedOrderItem(id, sender);
        return Ok(orderItem);
    }

    // POST: api/Order/{orderId}/Items
    [HttpPost("{orderId}/Items")]
    public async Task<IActionResult> AddItemToOrder(int orderId, [FromBody] AddItemDTO addItemDTO)
    {
        User? sender = await GetUserFromToken();
        var orderItem = await _orderService.CreateAuthorizedOrderItem(orderId, addItemDTO, sender);
        return CreatedAtRoute("GetOrderById", new { id = orderId }, orderItem);
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}
    [HttpDelete("{orderId}/Items/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();
        await _orderService.DeleteAuthorizedOrderItem(orderId,orderItemId,sender);
        return Ok("Order item deleted successfully.");
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
