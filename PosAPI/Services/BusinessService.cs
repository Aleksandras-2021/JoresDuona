
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
        if (sender == null || sender.Role is UserRole.Manager or UserRole.Worker)
            throw new UnauthorizedAccessException("You are not authorized to access businesses.");

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
        if (sender == null || sender.Role is UserRole.Manager or UserRole.Worker)
            throw new UnauthorizedAccessException("You are not authorized to access this business.");

        if (businessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to access this business.");

        var business = await _businessRepository.GetBusinessByIdAsync(sender.BusinessId);

        if (business == null)
            throw new KeyNotFoundException($"Business with ID {businessId} not found.");

        return business;
    }

    public async Task UpdateAuthorizedBusinessAsync(Business business, User? sender)
    {
        if (sender == null || sender.Role is UserRole.Manager or UserRole.Worker)
            throw new UnauthorizedAccessException("You are not authorized to update business information.");

        if (business.Id != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("You are not authorized to update this business.");

        var existingBusiness = await _businessRepository.GetBusinessByIdAsync(business.Id);
        if (existingBusiness == null)
            throw new KeyNotFoundException($"Business with ID {business.Id} not found.");

        await _businessRepository.UpdateBusinessAsync(business);
    }

    public async Task CreateAuthorizedBusinessAsync(Business business, User? sender)
    {
        if (business == null) throw new ArgumentNullException(nameof(business));
        if (sender == null) throw new ArgumentNullException(nameof(sender));

        if (sender is not { Role: UserRole.SuperAdmin })
            throw new UnauthorizedAccessException("You are not authorized to Create Business.");

        await _businessRepository.AddBusinessAsync(business);
    }
    public async Task DeleteAuthorizedBusinessAsync(int businessId, User? sender)
    {
        if (sender == null) throw new UnauthorizedAccessException(nameof(sender));

        if (sender is not { Role: UserRole.SuperAdmin })
            throw new UnauthorizedAccessException("You are not authorized to Create Business.");

        await _businessRepository.DeleteBusinessAsync(businessId);
    }
}