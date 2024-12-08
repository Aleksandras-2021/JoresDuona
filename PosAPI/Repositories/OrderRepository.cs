using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
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

        // Check if BusinessId exists in the table
        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == order.BusinessId);

        if (!businessExists)
        {
            throw new Exception($"Business with ID {order.BusinessId} does not exist.");
        }
        if (order.User == null)
            order.User = await _context.Users.FindAsync(order.UserId);

        try
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new order to the database.", ex);
        }

    }
    public async Task<List<Order>> GetAllBusinessOrdersAsync(int businessId)
    {
        return await _context.Set<Order>()
            .Where(order => order.BusinessId == businessId)
            .OrderBy(order => order.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Set<Order>()
            .OrderBy(order => order.CreatedAt)
            .ToListAsync();
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

        Order order = await _context.Orders.FindAsync(orderItem.OrderId);


        if (orderItem.Order == null)
            orderItem.Order = order;

        if (orderItem.Item == null)
            orderItem.Item = await _context.Items.FindAsync(orderItem.ItemId);

        //decrease quantity by 1
        _context.Items.Find(orderItem.ItemId).Quantity -= orderItem.Quantity;

        try
        {
            await _context.OrderItems.AddAsync(orderItem);
            order.OrderItems.Add(orderItem);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new orderItem to the database.", ex);
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

        // Fetch the ItemVariation if it's not already populated
        if (variation.ItemVariation == null)
            variation.ItemVariation = await _context.ItemVariations.FindAsync(variation.ItemVariationId);

        // Find the associated OrderItem
        OrderItem orderItem = await _context.OrderItems.FindAsync(variation.OrderItemId);

        if (variation.OrderItem == null)
            variation.OrderItem = orderItem;

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

            // Save changes to the database
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

    public async Task<List<ItemVariation>> GetSelectedVariationsForItemAsync(int itemId, int orderItemId)
    {
        return await _context.ItemVariations
            .Where(iv => iv.ItemId == itemId &&
                         _context.OrderItemVariations
                            .Any(oiv => oiv.ItemVariationId == iv.Id && oiv.OrderItemId == orderItemId))
            .ToListAsync();
    }




}
