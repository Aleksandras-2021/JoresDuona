using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Controllersq;
using PosAPI.Repositories;
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
    public async Task<List<Order>> GetAuthorizedOrders(User sender)
    {
        List<Order>? orders = null;

        if (sender.Role == UserRole.SuperAdmin)
            orders = await _orderRepository.GetAllOrdersAsync();
        else if (sender.Role == UserRole.Manager || sender.Role == UserRole.Owner || sender.Role == UserRole.Worker)
            orders = await _orderRepository.GetAllBusinessOrdersAsync(sender.BusinessId);
        else
            orders = new List<Order>();


        return orders;
    }
    public async Task<Order?> GetAuthorizedOrder(int orderId, User sender)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (sender.Role != UserRole.SuperAdmin && order.BusinessId != sender.BusinessId)
            throw new UnauthorizedAccessException();

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        return order;
    }

    public async Task<Order?> GetAuthorizedOrderForModification(int orderId, User sender)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (sender.Role != UserRole.SuperAdmin && order.BusinessId != sender.BusinessId)
            throw new UnauthorizedAccessException();

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        //Owner can modify orders & manager
        if (order.Status == OrderStatus.Closed && sender.Role == UserRole.Worker)
            throw new UnauthorizedAccessException("You are not authorized to modify closed orders.");

        return order;
    }


    public async Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender)
    {
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);

        if (orderItem == null)
            throw new KeyNotFoundException($"Order item with ID {orderItemId} not found.");

        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {orderItem.ItemId} not found.");

        if (item.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this order item.");

        return orderItem;
    }

    public async Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        if (order.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this order.");

        var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

        if (orderItems == null || !orderItems.Any())
            throw new KeyNotFoundException($"Order items for order with ID {orderId} not found.");

        return orderItems;
    }

    public async Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId, User sender)
    {
        var orderItemVariation = await _orderRepository.GetOrderItemVariationByIdAsync(variationId);

        if (orderItemVariation == null)
            throw new KeyNotFoundException($"Variation with ID {variationId} not found.");

        if (orderItemVariation.OrderItemId != orderItemId)
            throw new KeyNotFoundException($"Variation with ID {variationId} does not belong to OrderItem {orderItemId}.");

        // Fetch the associated OrderItem and Item for further validation
        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        var item = await _itemRepository.GetItemByIdAsync(orderItem.ItemId);

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {orderItem.ItemId} not found.");

        if (item.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this variation.");

        return orderItemVariation;
    }

    public async Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender)
    {
        var variation = await _itemRepository.GetItemVariationByIdAsync(variationId);

        if (variation == null)
            throw new KeyNotFoundException($"Variation with ID {variationId} not found.");

        var item = await _itemRepository.GetItemByIdAsync(variation.ItemId);

        if (item == null)
            throw new KeyNotFoundException($"Item with ID {variation.ItemId} not found.");

        if (item.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this item variation.");

        return variation;
    }

    public async Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender)
    {
        var orderItemVariations = await _orderRepository.GetOrderItemVariationsByOrderItemIdAsync(orderItemId);

        if (orderItemVariations == null || !orderItemVariations.Any())
            throw new KeyNotFoundException($"No variations found for order item with ID {orderItemId}.");

        var orderItem = await _orderRepository.GetOrderItemById(orderItemId);
        if (orderItem == null)
            throw new KeyNotFoundException($"Order item with ID {orderItemId} not found.");

        var order = await _orderRepository.GetOrderByIdAsync(orderItem.OrderId);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderItem.OrderId} not found.");

        if (order.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access variations for this order item.");

        return orderItemVariations;
    }


    public async Task RecalculateOrderCharge(int orderId)
    {
        Order order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order.Status == OrderStatus.Closed)
            return;

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
            order.ChargeAmount += variation.AdditionalPrice * variation.Quantity;
            OrderItem orderItemForVar = await _orderRepository.GetOrderItemById(variation.OrderItemId);

            tax = await _taxRepository.GetTaxByItemIdAsync(orderItemForVar.ItemId);

            if (tax.IsPercentage)
                order.TaxAmount += variation.AdditionalPrice * variation.Quantity * tax.Amount / 100;
            else
                order.TaxAmount += tax.Amount;
        }
        await _orderRepository.UpdateOrderAsync(order);
    }




}
