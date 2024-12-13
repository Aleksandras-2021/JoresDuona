using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public class TaxService: ITaxService
{
    private readonly ITaxRepository _taxRepository;
    
    public TaxService(ITaxRepository taxRepository)
    {
        _taxRepository = taxRepository;
    }
    
    public async Task<List<Tax>> GetAuthorizedTaxesAsync(User? sender)
    {
        if (sender is null)
            throw new UnauthorizedAccessException();
        
        List<Tax> taxes = new List<Tax>();

        if (sender.Role is UserRole.SuperAdmin)
            taxes = await _taxRepository.GetAllTaxesAsync();
        else if(sender.Role is UserRole.Manager or UserRole.Owner or UserRole.Worker)
            taxes = await _taxRepository.GetAllBusinessTaxesAsync(sender.BusinessId);

        return taxes;

    }

    public async Task<Tax> GetAuthorizedTaxByIdAsync(int taxId, User? sender)
    {
        if (sender is null)
            throw new UnauthorizedAccessException();

        Tax? tax = await _taxRepository.GetTaxByIdAsync(taxId);

        if (tax == null)
            throw new KeyNotFoundException();

        if (tax.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        return tax;

    }

    public async Task UpdateAuthorizedTaxAsync(Tax tax, User? sender)
    {
        ArgumentNullException.ThrowIfNull(tax);
        
        if (sender is null || sender.Role is UserRole.Worker)
            throw new UnauthorizedAccessException();
                    
        if(tax.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();
        
        await _taxRepository.UpdateTaxAsync(tax);
    }

    public async Task CreateAuthorizedTaxAsync(Tax tax, User? sender)
    {
        ArgumentNullException.ThrowIfNull(tax);
        
        if (sender is null || sender.Role is UserRole.Worker)
            throw new UnauthorizedAccessException();
                    
        if(tax.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        await _taxRepository.AddTaxAsync(tax);
    }

    public async Task DeleteAuthorizedTaxAsync(int taxId, User? sender)
    {
        if (sender is null || sender.Role is UserRole.Worker)
            throw new UnauthorizedAccessException();

        Tax? tax = await _taxRepository.GetTaxByIdAsync(taxId);
        if (tax == null)
            throw new KeyNotFoundException();
        if(tax.BusinessId != sender.BusinessId && sender.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException();

        await _taxRepository.DeleteTaxAsync(taxId);
    }
}