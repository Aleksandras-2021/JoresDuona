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
    Task AddOrderItemAsync(OrderItem orderItem);
    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);


}
