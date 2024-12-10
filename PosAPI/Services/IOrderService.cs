using PosShared.Models;

namespace PosAPI.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<Order?> GetAuthorizedOrder(int orderId, User sender);
    Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender);
    Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender);
    Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId, User sender);
    Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender);
    Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender);
    Task<List<ItemVariation>?> GetAuthorizedItemVariations(int orderItemId, User sender);

    Task RecalculateOrderCharge(int orderId);



}
