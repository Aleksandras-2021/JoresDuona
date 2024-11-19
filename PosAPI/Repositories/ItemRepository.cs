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


    }
}
