using PosShared;
using PosShared.Models;

namespace PosAPI.Services;

public interface IOrderService
{
    Task<PaginatedResult<Order>> GetAuthorizedOrders(User sender, int pageNumber, int pageSize);
    Task<int> CreateAuthorizedOrder(User? sender);

    Task<Order?> GetAuthorizedOrder(int orderId, User sender);
    Task<Order?> GetAuthorizedOrderForModification(int orderId, User sender);
    Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender);
    Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender);
    Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId, User sender);
    Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender);
    Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender);
    Task<List<OrderItemVariation>?> GetAuthorizedOrderVariations(int orderId, User sender);
    Task<List<PosShared.Models.OrderService>?> GetAuthorizedOrderServices(int orderId, User? sender);

    Task RecalculateOrderCharge(int orderId);
}
