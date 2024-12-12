using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Repositories.Interfaces;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddBusinessAsync(Business business)
    {
        ArgumentNullException.ThrowIfNull(business);
        try
        {
            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the new Business to the database.", ex);
        }
    }

    public async Task<PaginatedResult<Business>> GetAllBusinessAsync(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _context.Set<Business>().CountAsync();

        var businesses = await _context.Set<Business>()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(business => business.Id)
            .ToListAsync();
        return PaginatedResult<Business>.Create(businesses, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<Business>> GetPaginatedBusinessAsync(int businessId, int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _context.Set<Business>().CountAsync();

        var businesses = await _context.Set<Business>()
            .Where(b => b.Id == businessId)
            .OrderByDescending(business => business.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Business>.Create(businesses, totalCount, pageNumber, pageSize);
    }

    public async Task<Business> GetBusinessByIdAsync(int businessId)
    {
        var business = await _context.Set<Business>().FindAsync(businessId);
        if (business == null)
        {
            throw new KeyNotFoundException($"Business with ID {businessId} not found.");
        }

        return business;
    }

    public async Task UpdateBusinessAsync(Business business)
    {
        ArgumentNullException.ThrowIfNull(business);


        _context.Businesses.Update((business));
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBusinessAsync(int businessId)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId);
        if (business == null)
            throw new KeyNotFoundException($"Business with ID {businessId} not found.");

        _context.Businesses.Remove(business);

        await _context.SaveChangesAsync();
    }
}