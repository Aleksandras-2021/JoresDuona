using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Migrations;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class TaxRepository : ITaxRepository
    {
        private readonly ApplicationDbContext _context;
        public TaxRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTaxAsync(Tax tax)
        {
            if (tax == null)
            {
                throw new ArgumentNullException(nameof(tax));
            }

            // Check if BusinessId exists in the table
            var businessExists = await _context.Businesses.AnyAsync(b => b.Id == tax.BusinessId);

            if (!businessExists)
            {
                throw new Exception($"Tax with ID {tax.BusinessId} does not exist.");
            }

            if (tax.Business == null)
                tax.Business = await _context.Businesses.FindAsync(tax.BusinessId);

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

        public async Task<Tax> GetTaxByItemIdAsync(int itemId)
        {
            Item? item = await _context.Items.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Tax for item with ID {itemId} not found, because Item could not be found.");

            var tax = GetTaxByCategoryAsync(item.Category, item.BusinessId);

            if (tax == null)
            {
                throw new KeyNotFoundException($"Tax with for category {item.Category} not found.");
            }

            return await tax;

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
        public async Task<Tax> GetTaxByIdAsync(int id)
        {
            var tax = await _context.Set<Tax>().FindAsync(id);
            if (tax == null)
            {
                throw new KeyNotFoundException($"Tax with ID {id} not found.");
            }

            return tax;
        }

        public async Task UpdateTaxAsync(Tax tax)
        {
            if (tax == null)
            {
                throw new ArgumentNullException(nameof(tax));
            }

            var existingTax = await _context.Set<Tax>().FindAsync(tax.Id);
            if (existingTax == null)
            {
                throw new KeyNotFoundException($"Tax with ID {tax.Id} not found.");
            }

            existingTax.Id = tax.Id;
            existingTax.BusinessId = tax.BusinessId;
            existingTax.Business = tax.Business;
            existingTax.Name = tax.Name;
            existingTax.Amount = tax.Amount;
            existingTax.IsPercentage = tax.IsPercentage;
            existingTax.Category = tax.Category;

            if (existingTax.Business == null)
                existingTax.Business = await _context.Businesses.FindAsync(tax.BusinessId);



            _context.Set<Tax>().Update(existingTax);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTaxAsync(int id)
        {
            // Find the tax by ID
            var tax = await _context.Set<Tax>().FindAsync(id);
            if (tax == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }

            // Remove the tax
            _context.Set<Tax>().Remove(tax);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }


        public async Task<Tax?> GetTaxByCategoryAsync(PosShared.Models.Items.ItemCategory category, int businessId)
        {
            var tax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Category == category && t.BusinessId == businessId);

            if (tax == null)
            {
                return null;
            }

            return tax;
        }
    }
}
