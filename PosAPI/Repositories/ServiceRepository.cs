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
        var user = await _context.Users.FindAsync(service.EmployeeId);
        if (user == null)
            throw new DbUpdateException("Cannot add this user to this service");
        try
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occurred while adding the new service to the database.", ex);
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
            throw new KeyNotFoundException($"Service with ID {id} not found.");
        }
        return service;
    }

    public async Task UpdateServiceAsync(Service service)
    {
        try
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occurred while updating the service in the database.", ex);
        }
    }

    public async Task DeleteServiceAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            throw new KeyNotFoundException($"Service with ID {id} not found.");
        }

        try
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("An error occurred while deleting the service from the database.", ex);
        }
    }

}