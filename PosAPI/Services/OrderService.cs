using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosShared;
using PosShared.Models;

namespace PosAPI.Services;



public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ITaxRepository _taxRepository;

    public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository, ITaxRepository taxRepository)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _taxRepository = taxRepository;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _orderRepository.GetOrderByIdAsync(orderId);
    }
    public async Task<PaginatedResult<Order>> GetAuthorizedOrders(User sender, int pageNumber = 1, int pageSize = 10)
    {
        AuthorizationHelper.Authorize("Order", "List", sender);

        PaginatedResult<Order>? orders = null;
        if (sender.Role == UserRole.SuperAdmin)
            orders = await _orderRepository.GetAllOrdersAsync(pageNumber, pageSize);
        else if (sender.Role == UserRole.Manager ||
                 sender.Role == UserRole.Owner ||
                 sender.Role == UserRole.Worker)
            orders = await _orderRepository.GetAllBusinessOrdersAsync(sender.BusinessId, pageNumber, pageSize);
        else
            orders = PaginatedResult<Order>.Create(new List<Order>(), 0, pageNumber, pageSize);

        return orders;
    }
    public async Task<Order?> GetAuthorizedOrder(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,order.BusinessId ,sender.BusinessId, "Read");

        return order;
    }
    
    public async Task<int> CreateAuthorizedOrder(User? sender)
    {
        AuthorizationHelper.Authorize("Order", "Create", sender);
        Order newOrder = new Order()
        {
            BusinessId = sender.BusinessId,
            CreatedAt = DateTime.UtcNow.AddHours(2),
            ClosedAt = null,
            UserId = sender.Id,
            Status = OrderStatus.Open,
            ChargeAmount = 0,
            DiscountAmount = 0,
            TaxAmount = 0,
            TipAmount = 0,
            Payments = new List<Payment>(),
            OrderDiscounts = new List<OrderDiscount>()
        };        
        await _orderRepository.AddOrderAsync(newOrder);

        return newOrder.Id;
    }

    public async Task<Order?> GetAuthorizedOrderForModification(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Update", sender);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,order.BusinessId ,sender.BusinessId, "Update");

        //Owner can modify orders & manager
        if ((order.Status == OrderStatus.Closed || order.Status == OrderStatus.Paid) && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to modify closed orders.");

        return order;
    }


    public async Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Read");
        
        return orderItem;
    }

    public async Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender,order.BusinessId ,sender.BusinessId, "Read");
        
        var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        if (!orderItems.Any())
            throw new KeyNotFoundException($"Order items for order with ID {orderId} not found.");

        return orderItems;
    }

    public async Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItemVariation = await _orderRepository.GetOrderItemVariationByIdAsync(variationId);
        
        if (orderItemVariation.OrderItemId != orderItemId)
            throw new KeyNotFoundException($"Variation with ID {variationId} does not belong to OrderItem {orderItemId}.");

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Read");


        return orderItemVariation;
    }

    public async Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var variation = await _itemRepository.GetItemVariationByIdAsync(variationId);

        if (variation == null)
            throw new KeyNotFoundException($"Variation with ID {variationId} not found.");

        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender,item.BusinessId ,sender.BusinessId, "Read");

        return variation;
    }

    public async Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderItemIdAsync(orderItemId);

        if (!orderItemVariations.Any())
            throw new KeyNotFoundException($"No variations found for order item with ID {orderItemId}.");

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var order = await _orderRepository.GetOrderByIdAsync(orderItem.OrderId);
        
        AuthorizationHelper.ValidateOwnershipOrRole(sender,order.BusinessId ,sender.BusinessId, "Read");

        return orderItemVariations;
    }

    public async Task<List<OrderItemVariation>?> GetAuthorizedOrderVariations(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        List<OrderItemVariation> orderVariations = await _orderRepository.GetAllOrderItemVariationsAsync(orderId);
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (orderVariations == null || !orderVariations.Any())
            throw new KeyNotFoundException($"No variations found for order with ID {order}.");

        AuthorizationHelper.ValidateOwnershipOrRole(sender,order.BusinessId ,sender.BusinessId, "Read");

        return orderVariations;
    }


    public async Task RecalculateOrderCharge(int orderId)
    {
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Paid)
            return;

        List<OrderItem> orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);
        List<OrderItemVariation> orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderIdAsync(orderId);
        Tax? tax;

        order.ChargeAmount = 0;
        order.TaxAmount = 0;

        //base charge(untaxed)
        foreach (var item in orderItems)
        {
            order.ChargeAmount += item.Price * item.Quantity;


            tax = await _taxRepository.GetTaxByItemIdAsync(item.ItemId);

            if (tax != null)
            {
                if (tax.IsPercentage)
                    order.TaxAmount += item.Price * item.Quantity * tax.Amount / 100;
                else
                    order.TaxAmount += tax.Amount;
            }
            else
            {
                order.TaxAmount = 0;
            }
        }

        foreach (var variation in orderItemVariations)
        {
            order.ChargeAmount += variation.AdditionalPrice * variation.Quantity;
            OrderItem orderItemForVar = await _orderRepository.GetOrderItemById(variation.OrderItemId);

            tax = await _taxRepository.GetTaxByItemIdAsync(orderItemForVar.ItemId);
            if (tax != null)
            {
                if (tax.IsPercentage)
                    order.TaxAmount += variation.AdditionalPrice * variation.Quantity * tax.Amount / 100;
                else
                    order.TaxAmount += tax.Amount;
            }
            else
            {
                order.TaxAmount = 0;
            }
        }
        await _orderRepository.UpdateOrderAsync(order);
    }




}
