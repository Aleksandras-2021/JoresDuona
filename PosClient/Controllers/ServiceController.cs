using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using PosClient.Services;
using PosShared;
using PosShared.Utilities;
using PosShared.Models;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosShared.DTOs;
using PosShared.ViewModels;

namespace PosClient.Controllers;

public class ServiceController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IUserSessionService _userSessionService;
    
    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;
    public ServiceController(HttpClient httpClient, IUserSessionService userSessionService)
    {
        _httpClient = httpClient;
        _userSessionService = userSessionService;
    }

    // GET: Service/Index
    public async Task<IActionResult> Index()
    {
        try 
        {
            string? token = Request.Cookies["authToken"];
            Console.WriteLine($"Token: {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + "/api/Service";

            var response = await _httpClient.GetAsync(apiUrl);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var services = JsonSerializer.Deserialize<List<Service>>(responseContent,JsonOptions.Default);
                Console.WriteLine($"Deserialized services count: {services?.Count ?? 0}");
                return View(services ?? new List<Service>());
            }

            Console.WriteLine($"Request failed with status: {response.StatusCode}");
            TempData["Error"] = "Could not retrieve services.";
            return View(new List<Service>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in Index: {ex.Message}");
            TempData["Error"] = "An error occurred while retrieving services.";
            return View(new List<Service>());
        }
    }

    // GET: Service/Create
    public async Task<IActionResult> Create(int pageNumber = 1, int pageSize = 20)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var usersApiUrl = ApiRoutes.User.GetPaginated(pageNumber, pageSize);
        var userResponse = await _httpClient.GetAsync(usersApiUrl);

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

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        var apiUrl = _apiUrl + "/api/Service";
        var content = new StringContent(JsonSerializer.Serialize(service), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, content);

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
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Fetch service details
        var serviceApiUrl = _apiUrl + $"/api/Service/{id}";
        var serviceResponse = await _httpClient.GetAsync(serviceApiUrl);

        if (!serviceResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to fetch service details. Please try again. " + serviceResponse.StatusCode;
            return RedirectToAction("Index");
        }

        var serviceData = await serviceResponse.Content.ReadAsStringAsync();
        var service = JsonSerializer.Deserialize<Service>(serviceData, JsonOptions.Default);

        // Fetch users for selection
        var usersApiUrl = ApiRoutes.User.GetPaginated(pageNumber, pageSize);
        var usersResponse = await _httpClient.GetAsync(usersApiUrl);

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

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Prepare Service DTO
        var serviceDto = new ServiceCreateDTO
        {
            Name = model.Name,
            Description = model.Description,
            BasePrice = model.BasePrice,
            DurationInMinutes = model.DurationInMinutes,
            EmployeeId = model.EmployeeId // Include selected employee
        };

        var apiUrl = _apiUrl + $"/api/Service/{model.Id}";
        var content = new StringContent(JsonSerializer.Serialize(serviceDto), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error updating service: {errorMessage}");

        // Re-fetch users to show in the dropdown if there's an error
        var usersApiUrl = ApiRoutes.User.GetPaginated(1, 20);
        var usersResponse = await _httpClient.GetAsync(usersApiUrl);
        
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
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Service/{id}";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var serviceData = await response.Content.ReadAsStringAsync();
            var service = JsonSerializer.Deserialize<Service>(serviceData);

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
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Service/{id}";
        var response = await _httpClient.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Unable to delete service. Please try again.";
        return RedirectToAction("Index");
    }
}
