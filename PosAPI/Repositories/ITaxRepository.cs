using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosShared.Models;

namespace PosAPI.Repositories;


public interface ITaxRepository
{
    // Tax
    Task AddTaxAsync(Tax tax);
    Task<List<Tax>> GetAllTaxesAsync();
    Task<List<Tax>> GetAllBusinessTaxesAsync(int businessId);
    Task<Tax> GetTaxByIdAsync(int id);
    Task UpdateTaxAsync(Tax tax);
    Task DeleteTaxAsync(int id);


}
