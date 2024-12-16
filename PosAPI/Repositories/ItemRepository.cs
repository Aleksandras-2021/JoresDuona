using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new item
        public async Task AddItemAsync(Item item)
        {
            var businessExists = await _context.Businesses.AnyAsync(b => b.Id == item.BusinessId);

            if (!businessExists)
                throw new KeyNotFoundException($"Business with ID {item.BusinessId} does not exist.");

            item.Business ??= await _context.Businesses.FindAsync(item.BusinessId);

            try
            {
                await _context.Items.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the new item to the database.", ex);
            }
        }

        // Get paginated list of all items
        public async Task<PaginatedResult<Item>> GetAllItemsAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Items.CountAsync();
            var items = await _context.Items
                .OrderByDescending(item => item.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Item>.Create(items, totalCount, pageNumber, pageSize);
        }

        // Get paginated list of items for a specific business
        public async Task<PaginatedResult<Item>> GetAllBusinessItemsAsync(int businessId, int pageNumber, int pageSize)
        {
            var totalCount = await _context.Items.CountAsync(i => i.BusinessId == businessId);
            var items = await _context.Items
                .Where(item => item.BusinessId == businessId)
                .OrderByDescending(item => item.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResult<Item>.Create(items, totalCount, pageNumber, pageSize);
        }

        // Get an item by ID
        public async Task<Item> GetItemByIdAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
                throw new KeyNotFoundException($"Item with ID {id} not found.");

            return item;
        }

        // Update an item
        public async Task UpdateItemAsync(Item item)
        {
            var existingItem = await _context.Items.FindAsync(item.Id);

            if (existingItem == null)
                throw new KeyNotFoundException($"Item with ID {item.Id} not found.");

            _context.Items.Update(existingItem);
            await _context.SaveChangesAsync();
        }

        // Delete an item and its associated variations
        public async Task DeleteItemAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
                throw new KeyNotFoundException($"Item with ID {id} not found.");

            var variations = await _context.ItemVariations.Where(v => v.ItemId == id).ToListAsync();
            _context.ItemVariations.RemoveRange(variations);
            _context.Items.Remove(item);

            await _context.SaveChangesAsync();
        }

        // Get all variations for a specific item
        public async Task<List<ItemVariation>> GetItemVariationsAsync(int itemId)
        {
            return await _context.ItemVariations
                .Where(variation => variation.ItemId == itemId)
                .OrderBy(variation => variation.Id)
                .ToListAsync();
        }

        // Get a specific variation by ID
        public async Task<ItemVariation> GetItemVariationByIdAsync(int variationId)
        {
            var variation = await _context.ItemVariations.FindAsync(variationId);

            if (variation == null)
                throw new KeyNotFoundException($"ItemVariation with ID {variationId} not found.");

            return variation;
        }

        // Add a new variation
        public async Task AddItemVariationAsync(ItemVariation itemVariation)
        {
            var itemExists = await _context.Items.AnyAsync(item => item.Id == itemVariation.ItemId);

            if (!itemExists)
                throw new KeyNotFoundException($"Item with ID {itemVariation.ItemId} not found.");

            await _context.ItemVariations.AddAsync(itemVariation);
            await _context.SaveChangesAsync();
        }

        // Update an existing variation
        public async Task UpdateItemVariationAsync(ItemVariation itemVariation)
        {
            var existingVariation = await _context.ItemVariations.FindAsync(itemVariation.Id);

            if (existingVariation == null)
                throw new KeyNotFoundException($"ItemVariation with ID {itemVariation.Id} not found.");

            _context.ItemVariations.Update(existingVariation);
            await _context.SaveChangesAsync();
        }

        // Delete a variation by ID
        public async Task DeleteItemVariationAsync(int variationId)
        {
            var variation = await _context.ItemVariations.FindAsync(variationId);

            if (variation == null)
                throw new KeyNotFoundException($"ItemVariation with ID {variationId} not found.");

            _context.ItemVariations.Remove(variation);
            await _context.SaveChangesAsync();
        }
    }
}

