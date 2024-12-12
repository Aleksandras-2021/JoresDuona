using PosShared;
using PosShared.Models;


namespace PosAPI.Services.Interfaces;

public interface IBusinessService
{
    Task<PaginatedResult<Business>> GetAuthorizedBusinessesAsync(User? sender, int pageNumber = 1, int pageSize = 10);
    Task<Business> GetAuthorizedBusinessByIdAsync(int businessId, User? sender);
    Task UpdateAuthorizedBusinessAsync(Business business, User? sender);
    Task CreateAuthorizedBusinessAsync(Business business, User? sender);
    Task DeleteAuthorizedBusinessAsync(int businessId, User? sender);
}