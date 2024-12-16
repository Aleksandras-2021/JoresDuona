using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public interface IOrderService
{
    Task<PaginatedResult<Order>> GetAuthorizedOrders(User sender, int pageNumber, int pageSize);
    Task<Order?> GetAuthorizedOrder(int orderId, User sender);
    Task<int> CreateAuthorizedOrder(User? sender);
    Task DeleteAuthorizedOrder(int orderId, User? sender);
    
    Task<OrderItem> CreateAuthorizedOrderItem(int orderId,AddItemDTO addItemDTO,User? sender);
    Task DeleteAuthorizedOrderItem(int orderId, int orderItemId, User? sender);
    Task UpdateAuthorizedOrder(Order order, User? sender);
    Task<OrderItem?> GetAuthorizedOrderItem(int orderItemId, User sender);
    Task<List<OrderItem>?> GetAuthorizedOrderItems(int orderId, User sender);
    
    Task<OrderItemVariation?> GetAuthorizedOrderItemVariation(int variationId, int orderItemId, User sender);
    Task<ItemVariation?> GetAuthorizedItemVariation(int variationId, User sender);
    Task<OrderItemVariation> CreateAuthorizedOrderItemVariation(int orderId, int itemId, AddVariationDTO addVariationDTO, User sender);
    Task DeleteAuthorizedOrderItemVariation(int orderId, int orderItemId, int orderItemVariationId, User sender);

    Task<List<OrderItemVariation>?> GetAuthorizedOrderItemVariations(int orderItemId, User sender);
    Task<List<OrderItemVariation>?> GetAuthorizedOrderVariations(int orderId, User sender);
    Task<List<PosShared.Models.OrderService>?> GetAuthorizedOrderServices(int orderId, User? sender);
}
