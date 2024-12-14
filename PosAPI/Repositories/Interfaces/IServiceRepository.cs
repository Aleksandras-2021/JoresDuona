using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosShared.Models;

namespace PosAPI.Repositories;

public interface IServiceRepository
{
    Task AddServiceAsync(Service service);
    Task<Service> GetServiceByIdAsync(int id);
    Task<List<Service>> GetAllServicesAsync();
    Task<List<Service>> GetAllBusinessServicesAsync(int businessId);
    Task UpdateServiceAsync(Service service);
    Task DeleteServiceAsync(int id);

}