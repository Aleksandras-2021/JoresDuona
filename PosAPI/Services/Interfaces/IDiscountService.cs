using PosShared.Models;

namespace PosAPI.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<IEnumerable<Discount>> GetAllAsync();
        Task<Discount> GetByIdAsync(int id);
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(int id);
        Task<Discount?> GetActiveDiscountByNameAsync(string discountName);
    }
}
