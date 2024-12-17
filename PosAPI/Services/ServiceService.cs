using Microsoft.EntityFrameworkCore;
using System.Transactions;
using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public class ServiceService: IServiceService
{
    private readonly IServiceRepository _serviceRepository;
    public ServiceService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<List<Service>> GetAuthorizedServices(User? sender)
    {
        AuthorizationHelper.Authorize("Service", "List", sender);
        List<Service> services;

        if (sender.Role == UserRole.SuperAdmin)
        {
            services = await _serviceRepository.GetAllServicesAsync();
        }
        else
        {
            services = await _serviceRepository.GetAllBusinessServicesAsync(sender.BusinessId);
        }

        if (!services.Any())
            throw new KeyNotFoundException("No services found");

        return services;
    }
    
    public async Task<Service> GetAuthorizedService(int id,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Read", sender);
        var service = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, service.BusinessId, sender.BusinessId, "Read");

        return service;
    }
    
    public async Task<Service> CreateAuthorizedService(ServiceCreateDTO service,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Create", sender);

        Service newService = new Service()
        {
            BusinessId = sender.BusinessId,
            Name = service.Name,
            Description = service.Description,
            EmployeeId = service.EmployeeId,
            BasePrice = service.BasePrice,
            DurationInMinutes = service.DurationInMinutes,
            Category = service.Category
        };
        
        await _serviceRepository.AddServiceAsync(newService);
        
        return newService;
    }
    
    public async Task UpdateAuthorizedService(int id,ServiceCreateDTO service,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Create", sender);
        Service? existingService = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, existingService.BusinessId, sender.BusinessId, "Update");

        existingService.Name = service.Name;
        existingService.Description = service.Description;
        existingService.EmployeeId = service.EmployeeId;
        existingService.BasePrice = service.BasePrice;
        existingService.DurationInMinutes = service.DurationInMinutes;
        existingService.Category = service.Category;
        
        await _serviceRepository.UpdateServiceAsync(existingService);
    }
    
    public async Task DeleteAuthorizedService(int id,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Delete", sender);
        Service? existingService = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, existingService.BusinessId, sender.BusinessId, "Delete");

        await _serviceRepository.DeleteServiceAsync(id);
    }


    
}