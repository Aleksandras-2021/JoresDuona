using System.Collections.Generic;
using System.Threading.Tasks;
using PosShared.Models;

namespace PosAPI.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllAsync();
        Task<Discount> GetByIdAsync(int id);
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(int id);
        Task<Discount?> GetActiveDiscountByNameAsync(string discountName);
    }
}
