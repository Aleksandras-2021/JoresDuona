using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Middlewares;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Order Management
    public async Task AddOrderAsync(Order order)
    {
        var user = await _context.Users.FindAsync(order.UserId) 
                    ?? throw new KeyNotFoundException($"User with ID {order.UserId} does not exist for order creation.");

        order.User = user;

        try
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occurred while adding the new order to the database.", ex);
        }
    }

    public async Task<PaginatedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Orders.CountAsync();
        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(orders, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<Order>> GetAllBusinessOrdersAsync(int businessId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Orders.CountAsync(o => o.BusinessId == businessId);
        var orders = await _context.Orders
            .Where(o => o.BusinessId == businessId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(orders, totalCount, pageNumber, pageSize);
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Order with ID {id} not found.");
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.OrderItemVariations)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        if (order.Status == OrderStatus.Open)
        {
            foreach (var orderItem in order.OrderItems)
            {
                var item = await _context.Items.FindAsync(orderItem.ItemId);
                if (item != null) item.Quantity += orderItem.Quantity;
            }
        }

        _context.OrderItemVariations.RemoveRange(order.OrderItems.SelectMany(oi => oi.OrderItemVariations));
        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    // Order Item Management
    public async Task AddOrderItemAsync(OrderItem orderItem)
    {
        var order = await _context.Orders.FindAsync(orderItem.OrderId)
                   ?? throw new KeyNotFoundException($"Order with ID {orderItem.OrderId} not found.");

        var item = await _context.Items.FindAsync(orderItem.ItemId)
                   ?? throw new KeyNotFoundException($"Item with ID {orderItem.ItemId} not found.");

        if (item.Quantity < orderItem.Quantity)
        {
            throw new BusinessRuleViolationException($"Not enough stock available to fulfill the order. for item {item.Id}");
        }

        item.Quantity -= orderItem.Quantity;
        orderItem.Order = order;
        orderItem.Item = item;

        try
        {
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occurred while adding the new order item to the database.", ex);
        }
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .Include(oi => oi.Item)
            .ToListAsync();

        return orderItems;
    }

    public async Task<OrderItem> GetOrderItemById(int orderItemId)
    {
        return await _context.OrderItems.FindAsync(orderItemId) 
            ?? throw new KeyNotFoundException($"Order item with ID {orderItemId} not found.");
    }

    public async Task DeleteOrderItemAsync(int id)
    {
        var orderItem = await _context.OrderItems
            .Include(oi => oi.OrderItemVariations)
            .FirstOrDefaultAsync(oi => oi.Id == id)
            ?? throw new KeyNotFoundException($"Order item with ID {id} not found.");

        var item = await _context.Items.FindAsync(orderItem.ItemId);
        if (item != null) item.Quantity += orderItem.Quantity;

        _context.OrderItemVariations.RemoveRange(orderItem.OrderItemVariations);
        _context.OrderItems.Remove(orderItem);

        await _context.SaveChangesAsync();
    }

    // Order Item Variation Management
    public async Task AddOrderItemVariationAsync(OrderItemVariation variation)
    {
        var orderItem = await _context.OrderItems.FindAsync(variation.OrderItemId)
                        ?? throw new KeyNotFoundException($"Order item with ID {variation.OrderItemId} not found.");

        var existingVariation = await _context.OrderItemVariations
            .FirstOrDefaultAsync(v => v.OrderItemId == variation.OrderItemId && v.ItemVariationId == variation.ItemVariationId);

        if (existingVariation != null)
        {
            existingVariation.Quantity += variation.Quantity;
        }
        else
        {
            await _context.OrderItemVariations.AddAsync(variation);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderItemIdAsync(int orderItemId)
    {
        return await _context.OrderItemVariations
            .Where(oiv => oiv.OrderItemId == orderItemId)
            .OrderBy(oiv => oiv.Id)
            .ToListAsync();
    }

    public async Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderIdAsync(int orderId)
    {
        return await _context.OrderItemVariations
            .Include(oiv => oiv.OrderItem)
            .Where(oiv => oiv.OrderItem.OrderId == orderId)
            .OrderBy(oiv => oiv.Id)
            .ToListAsync();
    }


    public async Task DeleteOrderItemVariationAsync(int varId)
    {
        var variation = await _context.OrderItemVariations.FindAsync(varId)
                        ?? throw new KeyNotFoundException($"Variation with ID {varId} not found.");

        _context.OrderItemVariations.Remove(variation);
        await _context.SaveChangesAsync();
    }

    public async Task<OrderItemVariation?> GetOrderItemVariationByIdAsync(int variationId)
    {
        return await _context.OrderItemVariations
            .Include(oiv => oiv.ItemVariation)
            .FirstOrDefaultAsync(oiv => oiv.Id == variationId);
    }

    public async Task<List<ItemVariation>> GetSelectedVariationsForItemAsync(int itemId, int orderItemId)
    {
        return await _context.ItemVariations
            .Where(iv => iv.ItemId == itemId &&
                         _context.OrderItemVariations
                             .Any(oiv => oiv.ItemVariationId == iv.Id && oiv.OrderItemId == orderItemId))
            .ToListAsync();
    }

    public async Task<List<OrderItemVariation>> GetAllOrderItemVariationsAsync(int orderId)
    {
        return await _context.OrderItemVariations
            .Where(oiv => _context.OrderItems
                .Any(oi => oi.Id == oiv.OrderItemId && oi.OrderId == orderId))
            .ToListAsync();
    }
    
    public async Task<ItemVariation?> GetItemVariationByIdAsync(int variationId)
    {
        return await _context.ItemVariations.FirstOrDefaultAsync(v => v.Id == variationId);
    }
    
    public async Task AddOrderServiceAsync(OrderService orderService)
    {
        await _context.OrderServices.AddAsync(orderService);
        await _context.SaveChangesAsync(); 
    }
    
    public async Task<List<OrderService>> GetAllOrderServices(int orderId)
    {
        return await _context.OrderServices
            .Where(os => os.OrderId == orderId)
            .ToListAsync();
    }
    public async Task<Order?> GetOrderByIdWithPaymentsAsync(int orderId)
    {
        return await _context.Orders
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<decimal> GetTotalPaymentsForOrderAsync(int orderId)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId)
            .SumAsync(p => p.Amount);
    }

    
}
