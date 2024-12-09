using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosShared.Models;

namespace PosAPI.Repositories;

public interface IOrderRepository
{
    Task AddOrderAsync(Order order);
    Task<List<Order>> GetAllBusinessOrdersAsync(int businessId);
    Task<Order> GetOrderByIdAsync(int id);
    Task UpdateOrderAsync(Order order);
    Task<List<Order>> GetAllOrdersAsync();
    Task DeleteOrderAsync(int orderId);
    Task AddOrderItemAsync(OrderItem orderItem);
    Task<OrderItem> GetOrderItemById(int orderItemId);
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderItemIdAsync(int orderItemId);
    Task DeleteOrderItemAsync(int id);
    Task AddOrderItemVariationAsync(OrderItemVariation variation);
    Task<ItemVariation?> GetItemVariationByIdAsync(int variationId);
    Task<OrderItemVariation?> GetOrderItemVariationByIdAsync(int variationId);
    Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderIdAsync(int orderId);
    Task<List<ItemVariation>> GetSelectedVariationsForItemAsync(int itemId, int orderItemId);





}
