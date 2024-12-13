using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Migrations;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class TaxRepository : ITaxRepository
    {
        private readonly ApplicationDbContext _context;
        public TaxRepository(ApplicationDbContext context) =>_context = context;
        
        public async Task AddTaxAsync(Tax tax)
        {
            Tax? taxForCategory = await GetTaxByCategoryAsync(tax.Category,tax.BusinessId);

            if (taxForCategory != null)
                throw new Exception($"Tax with category {tax.Category.ToString()} already exists ");

            try
            {
                await _context.Taxes.AddAsync(tax);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding the new Tax to the database.", ex);
            }

        }

        public async Task<Tax?> GetTaxByItemIdAsync(int itemId)
        {
            Item? item = await _context.Items.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Item with id {itemId} not found");


            Tax? tax = await GetTaxByCategoryAsync(item.Category, item.BusinessId);

            return  tax;
        }

        public async Task<List<Tax>> GetAllTaxesAsync()
        {
            return await _context.Set<Tax>()
               .OrderBy(tax => tax.Id)
               .ToListAsync();
        }
        public async Task<List<Tax>> GetAllBusinessTaxesAsync(int businessId)
        {
            return await _context.Set<Tax>()
                     .Where(tax => tax.BusinessId == businessId)
                     .OrderBy(tax => tax.Id)
                     .ToListAsync();
        }
        public async Task<Tax?> GetTaxByIdAsync(int id)
        {
            Tax? tax = await _context.Set<Tax>().FindAsync(id);

            return tax;
        }

        public async Task UpdateTaxAsync(Tax tax)
        {
           var existingTax = await _context.Set<Tax>()
                .Include(t => t.Business)
                .FirstOrDefaultAsync(t => t.Id == tax.Id);
           
            if (existingTax == null)
            {
                throw new KeyNotFoundException($"Tax with ID {tax.Id} not found.");
            }
            //basically ensures no duplicate taxes with same Business ID and category exist
            Tax? taxForCategory = await GetTaxByCategoryAsync(tax.Category,tax.BusinessId);
        
            if (taxForCategory != null && taxForCategory != existingTax)
                throw new Exception($"Tax with category {tax.Category.ToString()} already exists ");
            
            _context.Set<Tax>().Update(existingTax);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTaxAsync(int id)
        {
            var tax = await _context.Set<Tax>().FindAsync(id);
            if (tax == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }

            _context.Set<Tax>().Remove(tax);

            await _context.SaveChangesAsync();
        }


        public async Task<Tax?> GetTaxByCategoryAsync(PosShared.Models.Items.ItemCategory category, int businessId)
        {
            var tax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Category == category && t.BusinessId == businessId);

            return tax ?? null;
        }
        
    }

}
