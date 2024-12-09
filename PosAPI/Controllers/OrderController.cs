using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Repositories;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;
using System.Runtime.InteropServices;

namespace PosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ITaxRepository _taxRepository;
    private readonly ILogger<OrderController> _logger;
    public OrderController(IOrderRepository orderRepository, IUserRepository userRepository, IItemRepository itemRepository, ITaxRepository taxRepository, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _itemRepository = itemRepository;
        _taxRepository = taxRepository;
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
            await RecalculateOrderCharge(id);


            return Ok(order);
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
    //Order/{id}/UpdateStatus
    [HttpPost("{orderId}/UpdateStatus")]
    public async Task<IActionResult> UpdateStatus([FromQuery] int orderId, OrderStatus status)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound($"Order with ID {orderId} not found.");
            if (order.Status == OrderStatus.Closed)
                return Unauthorized("You are not authorized to modify closed order.");

            // Update the status
            order.Status = status;
            await _orderRepository.UpdateOrderAsync(order);

            return Ok(new { message = "Order status updated successfully.", status });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating order status: {ex.Message}");
            return StatusCode(500, "Internal server error.");
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
        if (order.Status == OrderStatus.Closed)
            return Unauthorized("You are not authorized to modify closed order.");

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

            await RecalculateOrderCharge(orderItem.OrderId);
            await _orderRepository.UpdateOrderAsync(order);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderItem);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    // POST: api/Order/{orderId}/DeleteItem/{orderItemId}
    [HttpDelete("{orderId}/DeleteItem/{orderItemId}")]
    public async Task<IActionResult> DeleteOrderItem(int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
        {
            return Unauthorized();
        }

        // Check if the order exists
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);

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
            await RecalculateOrderCharge(orderId);

            // Delete the order and its associated data
            await _orderRepository.DeleteOrderItemAsync(orderItemId);


            return Ok("Order Item deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting order item with ID {orderId}: {ex.Message}");
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
            if (order.Status == OrderStatus.Closed)
                existingOrder.ClosedAt = DateTime.UtcNow;
            //existingOrder.ChargeAmount = order.ChargeAmount;
            // existingOrder.DiscountAmount = order.DiscountAmount;
            //existingOrder.TaxAmount = order.TaxAmount;
            //existingOrder.TipAmount = order.TipAmount;

            await _orderRepository.UpdateOrderAsync(existingOrder);
            await RecalculateOrderCharge(existingOrder.Id);

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




    [HttpGet("{orderId}/OrderItems")]
    public async Task<IActionResult> GetOrderItems(int orderId)
    {
        User? sender = await GetUserFromToken();

        var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (sender == null || sender.BusinessId != order.BusinessId)
        {
            return Unauthorized();
        }

        if (orderItems == null || !orderItems.Any())
        {
            return NotFound("No items found for this order.");
        }

        return Ok(orderItems);
    }




    [HttpGet("{orderId}/OrderItems/{orderItemId}/OrderItemVariations")]
    public async Task<IActionResult> GetOrderItemVariations(int orderItemId)
    {
        User? sender = await GetUserFromToken();

        var orderItemVariatons = await _orderRepository.GetOrderItemVariationsByOrderItemIdAsync(orderItemId);
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);

        //Not clean, but fixes the problem atm
        if (orderItem.Item == null)
            orderItem.Item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);


        //

        if (sender == null || sender.BusinessId != orderItem.Item.BusinessId)
        {
            return Unauthorized();
        }

        if (orderItem == null)
        {
            return NotFound("No items found for this order.");
        }

        return Ok(orderItemVariatons);
    }

    //The main difference between this and method above
    //is that this returns an object of ItemVariation instead of OrderItemVariation
    [HttpGet("{orderId}/OrderItems/{orderItemId}/ItemVariations")]
    public async Task<IActionResult> GetItemVariations( int orderId, int orderItemId)
    {
        User? sender = await GetUserFromToken();
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (sender == null || sender.BusinessId != order.BusinessId)
        {
            return Unauthorized();
        }

        if (orderItem == null)
        {
            return NotFound("No items found for this order.");
        }

        List<ItemVariation> itemVariatons = await _orderRepository.GetSelectedVariationsForItemAsync(orderItem.ItemId, orderItemId);

        if (orderItem.Item == null)
            orderItem.Item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);


        return Ok(itemVariatons);
    }

    [HttpGet("OrderItems/{id}")]
    public async Task<IActionResult> GetOrderItem(int id)
    {
        User? sender = await GetUserFromToken();

        var orderItem = await _orderRepository.GetOrderItemById(id);

        if (sender == null || sender.BusinessId != orderItem.Item.BusinessId)
        {
            return Unauthorized();
        }

        if (orderItem == null)
        {
            return NotFound("No items found for this order.");
        }

        return Ok(orderItem);
    }

    [HttpPost("{orderId}/OrderItems/{itemId}/AddVariation")]
    public async Task<IActionResult> AddOrderItemVariation(int orderId, int itemId, [FromBody] AddVariationDTO addVariationDTO)
    {
        // Step 1: Authenticate User
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        // Step 2: Validate Order and OrderItem
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound($"Order with ID {orderId} not found.");
        if (order.Status == OrderStatus.Closed)
            return Unauthorized("You are not authorized to modify closed order.");

        if (order.BusinessId != sender.BusinessId)
            return Unauthorized("You are not authorized to modify this order.");

        var orderItem = await _orderRepository.GetOrderItemById(itemId);
        if (orderItem == null || orderItem.OrderId != orderId)
            return NotFound($"Order Item with ID {itemId} not found in Order {orderId}.");

        // Step 3: Validate Item Variation
        var variation = await _itemRepository.GetItemVariationByIdAsync(addVariationDTO.VariationId);
        if (variation == null)
            return NotFound($"Variation with ID {addVariationDTO.VariationId} not found.");

        Item item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
        Tax tax = await _taxRepository.GetTaxByItemIdAsync(item.Id);

        // Step 4: Create and Add Order Item Variation
        OrderItemVariation orderItemVariation = new OrderItemVariation
        {
            OrderItemId = itemId,
            ItemVariationId = addVariationDTO.VariationId,
            Quantity = addVariationDTO.Quantity,
            AdditionalPrice = variation.AdditionalPrice,
            ItemVariation = variation,
            OrderItem = orderItem,

        };

        try
        {
            await _orderRepository.AddOrderItemVariationAsync(orderItemVariation);

            await _orderRepository.UpdateOrderAsync(order);

            // Step 5: Update Order's Charge Amount
            await RecalculateOrderCharge(orderId);

            return CreatedAtAction(nameof(GetOrderItemVariationById), new { orderId = orderId, orderItemId = itemId, variationId = orderItemVariation.Id }, orderItemVariation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Error adding order item variation: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    // GET: api/Order/{orderId}/OrderItems/{orderItemId}/Variations/{variationId}
    [HttpGet("{orderId}/OrderItems/{orderItemId}/Variations/{variationId}")]
    public async Task<IActionResult> GetOrderItemVariationById(int orderId, int orderItemId, int variationId)
    {
        // Step 1: Authenticate User
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        // Step 2: Validate Order and OrderItem
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound($"Order with ID {orderId} not found.");

        if (order.BusinessId != sender.BusinessId)
            return Unauthorized("You are not authorized to access this order.");

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        if (orderItem == null || orderItem.OrderId != orderId)
            return NotFound($"Order Item with ID {orderItemId} not found in Order {orderId}.");

        // Step 3: Retrieve OrderItemVariation
        var orderItemVariation = await _orderRepository.GetOrderItemVariationByIdAsync(variationId);
        if (orderItemVariation == null || orderItemVariation.OrderItemId != orderItemId)
            return NotFound($"Variation with ID {variationId} not found in Order Item {orderItemId}.");

        // Step 4: Return the Variation Details
        return Ok(orderItemVariation);
    }


    private async Task RecalculateOrderCharge(int orderId)
    {
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);
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
            order.ChargeAmount += variation.AdditionalPrice;
            OrderItem orderItemForVar = await _orderRepository.GetOrderItemById(variation.OrderItemId);

            tax = await _taxRepository.GetTaxByItemIdAsync(orderItemForVar.ItemId);

            if (tax.IsPercentage)
                order.TaxAmount += variation.AdditionalPrice * variation.Quantity * tax.Amount / 100;
            else
                order.TaxAmount += tax.Amount;

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
