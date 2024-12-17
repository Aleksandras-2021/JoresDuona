using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories;

public interface IOrderRepository
{
    //Order
    Task AddOrderAsync(Order order);
    Task<PaginatedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);
    Task<PaginatedResult<Order>> GetAllBusinessOrdersAsync(int businessId, int pageNumber, int pageSize);
    Task<Order> GetOrderByIdAsync(int id);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(int orderId);

    //Order Items
    Task AddOrderItemAsync(OrderItem orderItem);
    Task<OrderItem> GetOrderItemById(int orderItemId);
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderItemIdAsync(int orderItemId);
    Task DeleteOrderItemAsync(int id);
    Task DeleteOrderItemsAsync(int orderId);

    Task AddOrderItemVariationAsync(OrderItemVariation variation);
    Task DeleteOrderItemVariationAsync(int varId);
    Task DeleteOrderItemVariationsAsync(int orderId);

    //OrderItemVariations
    Task<OrderItemVariation?> GetOrderItemVariationByIdAsync(int variationId);
    Task<List<OrderItemVariation>> GetAllOrderItemVariationsAsync(int orderId);
    Task AddOrderServiceAsync(OrderService orderService);
    Task<List<OrderService>> GetAllOrderServices(int orderId);
    Task<Order?> GetOrderByIdWithPaymentsAsync(int orderId);
    
    //OrderServices
    Task DeleteOrderServicesAsync(int orderId);
}
