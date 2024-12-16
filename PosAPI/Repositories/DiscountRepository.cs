using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Repositories.Interfaces;
using PosShared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosAPI.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllAsync()
{
    return await _context.Discounts.ToListAsync();
}

        public async Task<Discount> GetByIdAsync(int id)
        {
            return await _context.Discounts.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Discount discount)
        {
            await _context.Discounts.AddAsync(discount);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var discount = await GetByIdAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Discount?> GetActiveDiscountByNameAsync(string discountName)
        {
            var now = DateTime.Now;

            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Description == discountName
                                          && now >= d.ValidFrom
                                          && now <= d.ValidTo);
        }



    }
}
