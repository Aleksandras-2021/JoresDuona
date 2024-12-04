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

        if (orderItem.Order == null)
            orderItem.Order = await _context.Orders.FindAsync(orderItem.OrderId);

        if (orderItem.Item == null)
            orderItem.Item = await _context.Items.FindAsync(orderItem.ItemId);

        try
        {
            await _context.OrderItems.AddAsync(orderItem);
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
    public async Task AddOrderItemVariationAsync(OrderItemVariation variation)
    {
        if (variation == null)
        {
            throw new ArgumentNullException(nameof(variation));
        }

        if (variation.ItemVariation == null)
            variation.ItemVariation = await _context.ItemVariations.FindAsync(variation.ItemVariationId);

        if (variation.OrderItem == null)
            variation.OrderItem = await _context.OrderItems.FindAsync(variation.OrderItemId);

        try
        {
            await _context.OrderItemVariations.AddAsync(variation);
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

}
