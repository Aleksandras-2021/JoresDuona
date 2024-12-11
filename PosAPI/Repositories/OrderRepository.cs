using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared;
using PosShared.Models;
using System.Security.Cryptography.X509Certificates;

namespace PosAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddOrderAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == order.BusinessId);

        if (!businessExists)
        {
            throw new Exception($"Business with ID {order.BusinessId} does not exist.");
        }

        var user = await _context.Users.FindAsync(order.UserId);
        if (user == null)
        {
            throw new Exception($"User with ID {order.UserId} does not exist.");
        }

        order.User = user;

        try
        {
            _context.Attach(user);

            // Add the order to the database
            await _context.Orders.AddAsync(order);

            // Save changes
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new order to the database.", ex);
        }
    }



    public async Task<PaginatedResult<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Set<Order>().CountAsync();

        var orders = await _context.Set<Order>()
            .OrderByDescending(order => order.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(orders, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<Order>> GetAllBusinessOrdersAsync(int businessId, int pageNumber, int pageSize)
    {
        var totalCount = await _context.Set<Order>()
            .Where(order => order.BusinessId == businessId)
            .CountAsync();

        var orders = await _context.Set<Order>()
            .Where(order => order.BusinessId == businessId)
            .OrderBy(order => order.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(orders, totalCount, pageNumber, pageSize);
    }


    public async Task<Order> GetOrderByIdAsync(int id)
    {
        var order = await _context.Set<Order>().FindAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found.");
        }

        return order;
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        // Find the order
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemVariations) // Include variations
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        // Remove associated variations
        foreach (var orderItem in order.OrderItems)
        {
            _context.OrderItemVariations.RemoveRange(orderItem.OrderItemVariations);
        }

        // Remove associated order items
        _context.OrderItems.RemoveRange(order.OrderItems);

        // Remove the order
        _context.Orders.Remove(order);

        // Save changes
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
    public async Task AddOrderItemAsync(OrderItem orderItem)
    {
        if (orderItem == null)
        {
            throw new ArgumentNullException(nameof(orderItem));
        }

        var order = await _context.Orders.FindAsync(orderItem.OrderId);
        if (order == null)
        {
            throw new Exception($"Order with ID {orderItem.OrderId} not found.");
        }

        var item = await _context.Items.FindAsync(orderItem.ItemId);
        if (item == null)
        {
            throw new Exception($"Item with ID {orderItem.ItemId} not found.");
        }

        orderItem.Order = order;
        orderItem.Item = item;

        if (item.Quantity < orderItem.Quantity)
        {
            throw new Exception("Not enough stock available to fulfill the order.");
        }
        item.Quantity -= orderItem.Quantity;

        try
        {
            _context.Attach(order);
            _context.Attach(item);
            await _context.OrderItems.AddAsync(orderItem);

            // Save changes
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new order item to the database.", ex);
        }
    }




    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        var orderItems = await _context.Set<OrderItem>()
            .Where(orderItem => orderItem.OrderId == orderId)
            .OrderBy(orderItem => orderItem.Id)
            .ToListAsync();

        foreach (var orderItem in orderItems)
        {
            if (orderItem.Item == null)
            {
                orderItem.Item = await _context.Set<Item>().FindAsync(orderItem.ItemId);
            }
        }

        return orderItems;
    }

    public async Task<OrderItem> GetOrderItemById(int orderItemId)
    {
        var orderItem = await _context.Set<OrderItem>().FindAsync(orderItemId);

        return orderItem;
    }
    public async Task AddOrderItemVariationAsync(OrderItemVariation variation)
    {
        if (variation == null)
        {
            throw new ArgumentNullException(nameof(variation));
        }

        if (variation.ItemVariation != null)
        {
            _context.Attach(variation.ItemVariation);
        }

        OrderItem orderItem = await _context.OrderItems.FindAsync(variation.OrderItemId);

        if (variation.OrderItem != null)
        {
            _context.Attach(variation.OrderItem);
        }

        if (orderItem == null)
        {
            throw new ArgumentNullException($"OrderItem with ID {variation.OrderItemId} not found.");
        }

        try
        {
            var existingVariation = await _context.OrderItemVariations
                .FirstOrDefaultAsync(oiv => oiv.OrderItemId == variation.OrderItemId && oiv.ItemVariationId == variation.ItemVariationId);

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
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new orderItemVariation to the database.", ex);
        }
    }

    public async Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderItemIdAsync(int orderItemId)
    {
        var orderItemsVariations = await _context.Set<OrderItemVariation>()
            .Where(orderItem => orderItem.OrderItemId == orderItemId)
            .OrderBy(orderItem => orderItem.Id)
            .ToListAsync();

        //very not clean, but fixes an issue for now
        foreach (var orderItem in orderItemsVariations)
        {
            if (orderItem.OrderItem == null)
            {
                orderItem.OrderItem = await _context.Set<OrderItem>().FindAsync(orderItemId);
            }

        }

        return orderItemsVariations;
    }

    public async Task<List<OrderItemVariation>> GetOrderItemVariationsByOrderIdAsync(int orderId)
    {


        var orderItemsVariations = await _context.Set<OrderItemVariation>()
            .Where(orderItemVar => orderItemVar.OrderItem.OrderId == orderId)
            .OrderBy(orderItem => orderItem.Id)
            .ToListAsync();


        return orderItemsVariations;
    }

    public async Task DeleteOrderItemAsync(int id)
    {
        var orderItem = await _context.Set<OrderItem>().FindAsync(id);
        if (orderItem == null)
        {
            throw new KeyNotFoundException($"Item with ID {id} not found.");
        }

        _context.Items.Find(orderItem.ItemId).Quantity += orderItem.Quantity;

        var variations = await _context.Set<OrderItemVariation>().Where(v => v.ItemVariationId == id).ToListAsync();


        // Remove all associated variations
        _context.Set<OrderItemVariation>().RemoveRange(variations);

        _context.Set<OrderItem>().Remove(orderItem);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrderItemVariationAsync(int varId)
    {
        var orderItemVariation = await _context.Set<OrderItemVariation>().FindAsync(varId);
        if (orderItemVariation == null)
        {
            throw new KeyNotFoundException($"Variation with ID {varId} not found.");
        };

        _context.Set<OrderItemVariation>().Remove(orderItemVariation);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task<ItemVariation?> GetItemVariationByIdAsync(int variationId)
    {
        return await _context.ItemVariations.FirstOrDefaultAsync(v => v.Id == variationId);
    }
    public async Task<OrderItemVariation?> GetOrderItemVariationByIdAsync(int variationId)
    {
        return await _context.OrderItemVariations
            .Include(v => v.ItemVariation)
            .FirstOrDefaultAsync(v => v.Id == variationId);
    }

    public async Task<List<OrderItemVariation>> GetAllOrderItemVariationsAsync(int orderId)
    {
        return await _context.OrderItemVariations
            .Where(oiv => _context.OrderItems
                .Any(oi => oi.Id == oiv.OrderItemId && oi.OrderId == orderId))
            .ToListAsync();
    }


    public async Task<List<ItemVariation>> GetSelectedVariationsForItemAsync(int itemId, int orderItemId)
    {
        return await _context.ItemVariations
            .Where(iv => iv.ItemId == itemId &&
                         _context.OrderItemVariations
                            .Any(oiv => oiv.ItemVariationId == iv.Id && oiv.OrderItemId == orderItemId))
            .ToListAsync();
    }




}
