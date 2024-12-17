using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/Order")] //Tech document specifies this
[ApiController]
public class OrderItemsVariationsController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUserTokenService _userTokenService;
    private readonly ILogger<OrderItemsVariationsController> _logger;

    public OrderItemsVariationsController(IUserTokenService userTokenService, IOrderService orderService, ILogger<OrderItemsVariationsController> logger)
    {
        _userTokenService = userTokenService;
        _orderService = orderService;
        _logger = logger;
    }


    [HttpGet("{orderId}/Items/{orderItemId}/Variations")]
    public async Task<IActionResult> GetOrderItemVariations(int orderItemId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var orderItemVariations = await _orderService.GetAuthorizedOrderItemVariations(orderItemId, sender);
        
        return Ok(orderItemVariations);
    }

    // GET: api/Order/{orderId}/Items/{orderItemId}/Variations/{variationId}
    [HttpGet("{orderId}/Items/{orderItemId}/Variations/{variationId}")]
    public async Task<IActionResult> GetOrderItemVariationById(int orderId, int orderItemId, int variationId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var orderItemVariation = await _orderService.GetAuthorizedOrderItemVariation(variationId, orderItemId, sender);
        
        return Ok(orderItemVariation);
    }

    [HttpPost("{orderId}/Items/{itemId}/Variations")]
    public async Task<IActionResult> AddOrderItemVariation(int orderId, int itemId, [FromBody] AddVariationDTO addVariationDTO)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var orderItemVariation =
                await _orderService.CreateAuthorizedOrderItemVariation(orderId, itemId, addVariationDTO, sender);
        
        return CreatedAtAction(nameof(GetOrderItemVariationById),
                new { orderId = orderId, orderItemId = itemId, variationId = orderItemVariation.Id },
                orderItemVariation);
    }

    // POST: api/Order/{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}
    [HttpDelete("{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}")]
    public async Task<IActionResult> DeleteOrderItemVariation(int orderId, int orderItemId, int orderItemVariationId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        await _orderService.DeleteAuthorizedOrderItemVariation(orderId, orderItemId, orderItemVariationId, sender);

        return Ok("Order Item Variation deleted successfully.");
    }
}
