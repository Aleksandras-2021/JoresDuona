using BCrypt.Net;
using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;



public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ITaxService _taxService;

    public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository, ITaxService taxService)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _taxService = taxService;
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

    public async Task<OrderItem> CreateAuthorizedOrderItem(int orderId, AddItemDTO addItemDTO, User? sender)
    {
        AuthorizationHelper.Authorize("Order", "Create", sender);
        AuthorizationHelper.Authorize("Items", "Read", sender);

        var item = await _itemRepository.GetItemByIdAsync(addItemDTO.ItemId);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Create");
        AuthorizationHelper.ValidateOwnershipOrRole(sender, item.BusinessId, sender.BusinessId, "Read");
      
        if (order.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        decimal taxedAmount = await _taxService.CalculateTaxByCategory(item.Price, 1, item.Category, item.BusinessId);

        OrderItem orderItem = new OrderItem
        {
            OrderId = orderId,
            ItemId = addItemDTO.ItemId,
            Quantity = 1, //or addItemDto.Quantity if that ever gets implemented
            Price = item.Price,
            TaxedAmount = taxedAmount
        };

        order.ChargeAmount += orderItem.Price * orderItem.Quantity;
        order.TaxAmount += orderItem.TaxedAmount * orderItem.Quantity;

        await _orderRepository.AddOrderItemAsync(orderItem);
        await _orderRepository.UpdateOrderAsync(order);

        return orderItem;
    }

    public async Task DeleteAuthorizedOrderItem(int orderId, int orderItemId, User? sender)
    {
        AuthorizationHelper.Authorize("Order", "Delete", sender);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Delete");
       
        if (order.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        var orderItem = await GetAuthorizedOrderItem(orderItemId, sender);

        order.ChargeAmount -= orderItem.Price * orderItem.Quantity;
        order.TaxAmount -= orderItem.TaxedAmount * orderItem.Quantity;

        var orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderItemIdAsync(orderItemId);

        foreach (var variation in orderItemVariations)
        {
            order.ChargeAmount -= variation.AdditionalPrice * variation.Quantity;
            order.TaxAmount -= variation.TaxedAmount * variation.Quantity;
        }
        
        await _orderRepository.DeleteOrderItemAsync(orderItemId);
    }

    public async Task<Order?> GetAuthorizedOrder(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Read");
        return order;
    }

    public async Task<Order> CreateAuthorizedOrder(User? sender)
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

        return newOrder;
    }

    public async Task UpdateAuthorizedOrder(Order order, User? sender)
    {
        AuthorizationHelper.Authorize("Order", "Update", sender);
        var existingOrder = await _orderRepository.GetOrderByIdAsync(order.Id);

        if (existingOrder.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        existingOrder.UserId = sender.Id; //Whoever updates order, takes over the ownership of it
        existingOrder.User = sender;
        existingOrder.Status = order.Status;
        existingOrder.ChargeAmount = order.ChargeAmount;
        existingOrder.DiscountAmount = order.DiscountAmount;
        existingOrder.TaxAmount = order.TaxAmount;
        existingOrder.TipAmount = order.TipAmount;
        
        await _orderRepository.UpdateOrderAsync(existingOrder);
    }

    
    public async Task DeleteAuthorizedOrder(int orderId, User? sender)
    {
        AuthorizationHelper.Authorize("Order", "Delete", sender);
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Delete");
        
        if (order.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        await _orderRepository.DeleteOrderItemVariationsAsync(orderId);
        await _orderRepository.DeleteOrderItemsAsync(orderId);
        await _orderRepository.DeleteOrderServicesAsync(orderId);
        
        await _orderRepository.DeleteOrderAsync(orderId);
    }

    public async Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, item.BusinessId, sender.BusinessId, "Read");

        return orderItem;
    }

    public async Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Read");

        var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        if (!orderItems.Any())
            orderItems = new List<OrderItem>();

        return orderItems;
    }

    public async Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId,
        User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItemVariation = await _orderRepository.GetOrderItemVariationByIdAsync(variationId);

        if (orderItemVariation.OrderItemId != orderItemId)
            throw new KeyNotFoundException(
                $"Variation with ID {variationId} does not belong to OrderItem {orderItemId}.");

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender, item.BusinessId, sender.BusinessId, "Read");
        
        return orderItemVariation;
    }

    public async Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var variation = await _itemRepository.GetItemVariationByIdAsync(variationId);

        if (variation == null)
            throw new KeyNotFoundException($"Variation with ID {variationId} not found.");

        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, item.BusinessId, sender.BusinessId, "Read");

        return variation;
    }

    public async Task<OrderItemVariation> CreateAuthorizedOrderItemVariation(int orderId, int itemId, AddVariationDTO addVariationDTO, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Create", sender);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Create");

        if (order.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        var variation = await GetAuthorizedItemVariation(addVariationDTO.VariationId, sender);
        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);

        decimal taxedAmount = await _taxService.CalculateTaxByCategory(variation.AdditionalPrice, addVariationDTO.Quantity, item.Category, sender.BusinessId);


        var orderItemVariation = new OrderItemVariation
        {
            OrderItemId = itemId,
            ItemVariationId = addVariationDTO.VariationId,
            Quantity = addVariationDTO.Quantity,
            AdditionalPrice = variation.AdditionalPrice * addVariationDTO.Quantity,
            TaxedAmount = taxedAmount
        };

        await _orderRepository.AddOrderItemVariationAsync(orderItemVariation);
            
        order.ChargeAmount += orderItemVariation.AdditionalPrice ;
        order.TaxAmount += orderItemVariation.TaxedAmount;
            
        await _orderRepository.UpdateOrderAsync(order);

        return orderItemVariation;
    }

    public async Task DeleteAuthorizedOrderItemVariation(int orderId, int orderItemId, int orderItemVariationId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Delete", sender);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Create");
        var orderItemVariation = await _orderRepository.GetOrderItemVariationByIdAsync(orderItemVariationId);
        
        if (order.Status is OrderStatus.Closed or OrderStatus.Paid or OrderStatus.Refunded)
            throw new BusinessRuleViolationException("Cannot modify closed order");
        
        order.ChargeAmount -= orderItemVariation.AdditionalPrice * orderItemVariation.Quantity;
        order.TaxAmount -= orderItemVariation.TaxedAmount * orderItemVariation.Quantity;
            
        await _orderRepository.DeleteOrderItemVariationAsync(orderItemVariationId);
    }

    public async Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);

        var orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderItemIdAsync(orderItemId);

        if (!orderItemVariations.Any())
            return new List<OrderItemVariation>();
        
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var order = await _orderRepository.GetOrderByIdAsync(orderItem.OrderId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Read");

        return orderItemVariations;
    }

    public async Task<List<OrderItemVariation>?> GetAuthorizedOrderVariations(int orderId, User sender)
    {
        AuthorizationHelper.Authorize("Order", "Read", sender);
        List<OrderItemVariation> orderVariations = await _orderRepository.GetAllOrderItemVariationsAsync(orderId);
        
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Read");

        if (!orderVariations.Any())
            orderVariations = new List<OrderItemVariation>();
        
        return orderVariations;
    }
    
    public async Task<List<PosShared.Models.OrderService>?> GetAuthorizedOrderServices(int orderId, User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Read", sender);
        List<PosShared.Models.OrderService> orderVariations = await _orderRepository.GetAllOrderServices(orderId);
        
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "Read");

        return orderVariations;
    }
}

