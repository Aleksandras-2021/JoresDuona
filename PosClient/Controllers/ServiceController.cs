using Microsoft.AspNetCore.Mvc;
using PosClient.Services;
using PosShared;
using PosShared.Models;
using System.Text.Json;
using System.Text;
using PosShared.DTOs;
using PosShared.ViewModels;

namespace PosClient.Controllers;

public class ServiceController : Controller
{
    private readonly ApiService _apiService;
    public ServiceController(ApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: Service/Index
    public async Task<IActionResult> Index()
    {
        try 
        {
            var apiUrl = ApiRoutes.Service.List;

            var response = await _apiService.GetAsync(apiUrl);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var services = JsonSerializer.Deserialize<List<Service>>(responseContent,JsonOptions.Default);
                return View(services ?? new List<Service>());
            }

            return View(new List<Service>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while retrieving services.";
            return View(new List<Service>());
        }
    }

    // GET: Service/Create
    public async Task<IActionResult> Create(int pageNumber = 1, int pageSize = 20)
    {
        var usersApiUrl = ApiRoutes.User.ListPaginated(pageNumber, pageSize);
        var userResponse = await _apiService.GetAsync(usersApiUrl);

        //Get all available users for service selection
        PaginatedResult<User>? users = null;
        if (userResponse.IsSuccessStatusCode)
        {
            var usersJson = await userResponse.Content.ReadAsStringAsync();
            users = JsonSerializer.Deserialize<PaginatedResult<User>>(usersJson,JsonOptions.Default);
        }
        else
        {
            users = new PaginatedResult<User>();
        }

        ServiceCreateViewModel model = new ServiceCreateViewModel()
        {
            Users = users,
        };

    return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ServiceCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // Service DTO
        var service = new ServiceCreateDTO
        {
            Name = model.Name,
            Description = model.Description,
            BasePrice = model.BasePrice,
            DurationInMinutes = model.DurationInMinutes,
            EmployeeId = model.EmployeeId,
            Category = model.Category
        };

        var apiUrl = ApiRoutes.Service.Create;
        var content = new StringContent(JsonSerializer.Serialize(service), Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating service: {errorMessage}");

        return RedirectToAction("Create");
    }


// GET: Service/Edit/
    public async Task<IActionResult> Edit(int id, int pageNumber = 1, int pageSize = 20)
    {
        // Fetch service details
        var serviceApiUrl = ApiRoutes.Service.GetById(id);
        var serviceResponse = await _apiService.GetAsync(serviceApiUrl);

        if (!serviceResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to fetch service details. Please try again. " + serviceResponse.StatusCode;
            return RedirectToAction("Index");
        }

        var serviceData = await serviceResponse.Content.ReadAsStringAsync();
        var service = JsonSerializer.Deserialize<Service>(serviceData, JsonOptions.Default);

        // Fetch users for selection
        var usersApiUrl = ApiRoutes.User.ListPaginated(pageNumber, pageSize);
        var usersResponse = await _apiService.GetAsync(usersApiUrl);

        PaginatedResult<User>? users = null;
        if (usersResponse.IsSuccessStatusCode)
        {
            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            users = JsonSerializer.Deserialize<PaginatedResult<User>>(usersJson, JsonOptions.Default);
        }
        else
        {
            users = new PaginatedResult<User>();
        }

        if (service != null)
        {
            var model = new ServiceCreateViewModel()
            {
                Id = id,
                Name = service.Name,
                Description = service.Description,
                DurationInMinutes = service.DurationInMinutes,
                BasePrice = service.BasePrice,
                EmployeeId = service.EmployeeId,
                Users = users
            };

            return View(model);
        }

        TempData["Error"] = "Service not found.";
        return RedirectToAction("Index");
    }

// POST: Service/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(ServiceCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // Return the view with validation errors
        }
        
        // Prepare Service DTO
        var serviceDto = new ServiceCreateDTO
        {
            Name = model.Name,
            Description = model.Description,
            BasePrice = model.BasePrice,
            DurationInMinutes = model.DurationInMinutes,
            EmployeeId = model.EmployeeId,
            Category = model.Category,
        };

        var apiUrl = ApiRoutes.Service.Update(model.Id);
        var content = new StringContent(JsonSerializer.Serialize(serviceDto), Encoding.UTF8, "application/json");

        var response = await _apiService.PutAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error updating service: {errorMessage}");

        // Re-fetch users to show in the dropdown if there's an error
        var usersApiUrl = ApiRoutes.User.ListPaginated(1, 20);
        var usersResponse = await _apiService.GetAsync(usersApiUrl);
        
        if (usersResponse.IsSuccessStatusCode)
        {
            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            model.Users = JsonSerializer.Deserialize<PaginatedResult<User>>(usersJson,JsonOptions.Default);
        }

        return View(model);
    }


    // GET: Service/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var apiUrl = ApiRoutes.Service.GetById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var serviceData = await response.Content.ReadAsStringAsync();
            var service = JsonSerializer.Deserialize<Service>(serviceData, JsonOptions.Default);

            if (service != null)
            {
                return View(service);
            }
        }

        return RedirectToAction("Index");
    }

    // POST: Service/Delete/
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var apiUrl = ApiRoutes.Service.Delete(id);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Unable to delete service. Please try again.";
        return RedirectToAction("Index");
    }
}
