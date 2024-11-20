using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
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

        public async Task AddItemAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            // Check if BusinessId exists in the table
            var businessExists = await _context.Businesses.AnyAsync(b => b.Id == item.BusinessId);

            if (!businessExists)
            {
                throw new Exception($"Business with ID {item.BusinessId} does not exist.");
            }

            try
            {
                await _context.Items.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding the new item to the database.", ex);
            }
        }


        public async Task<List<Item>> GetAllItemsAsync()
        {
            return await _context.Set<Item>()
                .OrderBy(item => item.Id)
                .ToListAsync();
        }

        public async Task<List<Item>> GetAllBusinessItemsAsync(int businessId)
        {
            return await _context.Set<Item>()
                .Where(item => item.BusinessId == businessId)
                .OrderBy(item => item.Id)
                .ToListAsync();
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            var item = await _context.Set<Item>().FindAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }

            return item;
        }

        public async Task UpdateItemAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var existingItem = await _context.Set<Item>().FindAsync(item.Id);
            if (existingItem == null)
            {
                throw new KeyNotFoundException($"Item with ID {item.Id} not found.");
            }

            existingItem.Name = item.Name;
            existingItem.BusinessId = item.BusinessId;
            existingItem.Description = item.Description;
            existingItem.BasePrice = item.BasePrice;
            existingItem.Price = item.Price;
            existingItem.Quantity = item.Quantity;

            _context.Set<Item>().Update(existingItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await _context.Set<Item>().FindAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {id} not found.");
            }

            _context.Set<Item>().Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeItemQuanityAsync(int? itemid, int newQuantity)
        {
            if (itemid == null)
            {
                throw new ArgumentException(nameof(itemid));
            }
            var item = await _context.Set<Item>().FindAsync(itemid);

            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {itemid} not found.");
            }

            item.Quantity = newQuantity;

            _context.Set<Item>().Update(item);

            await _context.SaveChangesAsync();
        }

        // Get all variations for a specific item
        public async Task<List<ItemVariation>> GetItemVariationsAsync(int itemId)
        {
            return await _context.Set<ItemVariation>()
                .Where(variation => variation.ItemId == itemId)
                .OrderBy(variation => variation.Id)
                .ToListAsync();
        }

        // Get a specific variation by ID
        public async Task<ItemVariation> GetItemVariationByIdAsync(int variationId)
        {
            var variation = await _context.Set<ItemVariation>().FindAsync(variationId);
            if (variation == null)
            {
                throw new KeyNotFoundException($"ItemVariation with ID {variationId} not found.");
            }

            return variation;
        }

        // Add a new variation
        public async Task AddItemVariationAsync(ItemVariation itemVariation)
        {
            if (itemVariation == null)
            {
                throw new ArgumentNullException(nameof(itemVariation));
            }

            // Ensure the related item exists
            var itemExists = await _context.Set<Item>().AnyAsync(item => item.Id == itemVariation.ItemId);
            if (!itemExists)
            {
                throw new KeyNotFoundException($"Item with ID {itemVariation.ItemId} not found.");
            }

            await _context.Set<ItemVariation>().AddAsync(itemVariation);
            await _context.SaveChangesAsync();
        }

        // Update an existing variation
        public async Task UpdateItemVariationAsync(ItemVariation itemVariation)
        {
            if (itemVariation == null)
            {
                throw new ArgumentNullException(nameof(itemVariation));
            }

            var existingVariation = await _context.Set<ItemVariation>().FindAsync(itemVariation.Id);
            if (existingVariation == null)
            {
                throw new KeyNotFoundException($"ItemVariation with ID {itemVariation.Id} not found.");
            }

            existingVariation.Name = itemVariation.Name;
            existingVariation.AdditionalPrice = itemVariation.AdditionalPrice;
            existingVariation.ItemId = itemVariation.ItemId;

            _context.Set<ItemVariation>().Update(existingVariation);
            await _context.SaveChangesAsync();
        }

        // Delete a variation by ID
        public async Task DeleteItemVariationAsync(int variationId)
        {
            var variation = await _context.Set<ItemVariation>().FindAsync(variationId);
            if (variation == null)
            {
                throw new KeyNotFoundException($"ItemVariation with ID {variationId} not found.");
            }

            _context.Set<ItemVariation>().Remove(variation);
            await _context.SaveChangesAsync();
        }


    }
}
