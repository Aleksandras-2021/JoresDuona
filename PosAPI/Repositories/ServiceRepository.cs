using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared.Models;

namespace PosAPI.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;
    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddServiceAsync(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // Check if BusinessId exists in the table
        var businessExists = await _context.Businesses.AnyAsync(b => b.Id == service.BusinessId);

        if (!businessExists)
        {
            throw new Exception($"Business with ID {service.BusinessId} does not exist.");
        }

        if (service.Business == null)
            service.Business = await _context.Businesses.FindAsync(service.BusinessId);

        try
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new service to the database.", ex);
        }
    }

    public async Task<List<Service>> GetAllServicesAsync()
    {
        return await _context.Set<Service>()
            .OrderBy(service => service.Id)
            .ToListAsync();
    }

    public async Task<List<Service>> GetAllBusinessServicesAsync(int businessId)
    {
        return await _context.Set<Service>()
            .Where(service => service.BusinessId == businessId)
            .OrderBy(service => service.Id)
            .ToListAsync();
    }

    public async Task<Service> GetServiceByIdAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            throw new Exception($"Service with ID {id} not found.");
        }
        return service;
    }

    public async Task UpdateServiceAsync(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        try
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while updating the service in the database.", ex);
        }
    }

    public async Task DeleteServiceAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            throw new Exception($"Service with ID {id} not found.");
        }

        try
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while deleting the service from the database.", ex);
        }
    }

}