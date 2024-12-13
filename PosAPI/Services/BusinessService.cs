
using PosAPI.Middlewares;
using PosAPI.Repositories.Interfaces;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.Models;


namespace PosAPI.Services;

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<PaginatedResult<Business>> GetAuthorizedBusinessesAsync(User? sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        AuthorizationHelper.Authorize("Businesses", "List", sender);


        PaginatedResult<Business> business = null;

        if (sender.Role == UserRole.SuperAdmin)
            business = await _businessRepository.GetAllBusinessAsync(pageNumber, pageSize);
        else if (sender.Role == UserRole.Owner)
        {
            business = await _businessRepository.GetPaginatedBusinessAsync(sender.BusinessId, pageNumber, pageSize);
        }
        else
            business = PaginatedResult<Business>.Create(new List<Business>(), 0, pageNumber, pageSize);

        return business;
    }

    public async Task<Business> GetAuthorizedBusinessByIdAsync(int businessId, User? sender)
    {
        AuthorizationHelper.Authorize("Businesses", "Read", sender);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,businessId,sender.BusinessId,"Update");

        Business? business = await _businessRepository.GetBusinessByIdAsync(businessId);
        
        return business;
    }

    public async Task UpdateAuthorizedBusinessAsync(Business business, User? sender)
    {
        AuthorizationHelper.Authorize("Businesses", "Update", sender);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,business.Id,sender.BusinessId,"Update");
        await _businessRepository.UpdateBusinessAsync(business);
    }

    public async Task CreateAuthorizedBusinessAsync(Business business, User? sender)
    {
        AuthorizationHelper.Authorize("Businesses", "Create", sender);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,business.Id,sender.BusinessId,"Create");

        await _businessRepository.AddBusinessAsync(business);
    }
    public async Task DeleteAuthorizedBusinessAsync(int businessId, User? sender)
    {
        AuthorizationHelper.Authorize("Businesses", "Delete", sender);

        await _businessRepository.DeleteBusinessAsync(businessId);
    }
}