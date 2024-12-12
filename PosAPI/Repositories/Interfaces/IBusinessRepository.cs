using PosShared;
using PosShared.Models;

namespace PosAPI.Repositories.Interfaces;

public interface IBusinessRepository
{
    Task AddBusinessAsync(Business business);
    Task<PaginatedResult<Business>> GetAllBusinessAsync(int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<Business>> GetPaginatedBusinessAsync(int businessId, int pageNumber = 1, int pageSize = 10);

    Task<Business> GetBusinessByIdAsync(int businessId);
    Task UpdateBusinessAsync(Business business);
    Task DeleteBusinessAsync(int businessId);

}