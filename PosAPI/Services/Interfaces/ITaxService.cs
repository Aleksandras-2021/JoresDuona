using PosShared;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Models.Items;

namespace PosAPI.Services.Interfaces;

public interface ITaxService
{
    Task<List<Tax>> GetAuthorizedTaxesAsync(User? sender);
    Task<Tax> GetAuthorizedTaxByIdAsync(int taxId, User? sender);
    Task UpdateAuthorizedTaxAsync(Tax tax, User? sender);
    Task CreateAuthorizedTaxAsync(Tax tax, User? sender);
    Task DeleteAuthorizedTaxAsync(int taxId, User? sender);

    Task<decimal> CalculateTaxByCategory(decimal itemPrice, int itemQuantity, ItemCategory itemCategory,
        int businessId);
}