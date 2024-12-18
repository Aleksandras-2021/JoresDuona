using PosAPI.Middlewares;
using PosAPI.Migrations;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.Models;
using ItemCategory = PosShared.Models.Items.ItemCategory;

namespace PosAPI.Services;

public class TaxService : ITaxService
{
    private readonly ITaxRepository _taxRepository;

    public TaxService(ITaxRepository taxRepository) => _taxRepository = taxRepository;

    public async Task<List<Tax>> GetAuthorizedTaxesAsync(User? sender)
    {
        AuthorizationHelper.Authorize("Tax", "List", sender);

        List<Tax> taxes = new List<Tax>();

        if (sender.Role is UserRole.SuperAdmin)
            taxes = await _taxRepository.GetAllTaxesAsync();
        else if (sender.Role is UserRole.Manager or UserRole.Owner or UserRole.Worker)
            taxes = await _taxRepository.GetAllBusinessTaxesAsync(sender.BusinessId);

        return taxes;
    }

    public async Task<Tax> GetAuthorizedTaxByIdAsync(int taxId, User? sender)
    {
        AuthorizationHelper.Authorize("Tax", "Read", sender);

        Tax? tax = await _taxRepository.GetTaxByIdAsync(taxId);

        if (tax == null)
            throw new KeyNotFoundException();

        AuthorizationHelper.ValidateOwnershipOrRole(sender, tax.BusinessId, sender.BusinessId, "Read");

        return tax;
    }

    public async Task UpdateAuthorizedTaxAsync(Tax tax, User? sender)
    {
        AuthorizationHelper.Authorize("Tax", "Update", sender);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, tax.BusinessId, sender.BusinessId, "Update");

        await _taxRepository.UpdateTaxAsync(tax);
    }

    public async Task CreateAuthorizedTaxAsync(Tax tax, User? sender)
    {
        AuthorizationHelper.Authorize("Tax", "Create", sender);

        await _taxRepository.AddTaxAsync(tax);
    }

    public async Task DeleteAuthorizedTaxAsync(int taxId, User? sender)
    {
        AuthorizationHelper.Authorize("Tax", "Delete", sender);
        Tax? tax = await _taxRepository.GetTaxByIdAsync(taxId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, tax.BusinessId, sender.BusinessId, "Delete");

        await _taxRepository.DeleteTaxAsync(taxId);
    }

    public async Task<decimal> CalculateTaxByCategory(decimal itemPrice, int itemQuantity, ItemCategory itemCategory, int businessId)
    {
        Tax? tax = await _taxRepository.GetTaxByCategoryAsync(itemCategory, businessId);

        decimal totalTaxAmount;
        if (tax != null && itemPrice > 0)
        {
            if (tax.IsPercentage)
                totalTaxAmount = itemPrice * itemQuantity * tax.Amount / 100;
            else
                totalTaxAmount = tax.Amount;
        }
        else
        {
            totalTaxAmount = 0;
        }

        return totalTaxAmount;
    }

}