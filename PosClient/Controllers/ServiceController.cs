using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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
            Console.WriteLine($"Calling API URL: {apiUrl}");

            var response = await _httpClient.GetAsync(apiUrl);
            Console.WriteLine($"Response Status: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var services = JsonSerializer.Deserialize<List<Service>>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            users = JsonSerializer.Deserialize<PaginatedResult<User>>(usersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            return View(model); // Return the view with validation errors
        }

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Prepare Service DTO
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
    public async Task<IActionResult> Edit(int id)
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

        TempData["Error"] = "Unable to fetch service details. Please try again.";
        return RedirectToAction("Index");
    }

    // POST: Service/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(Service service)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var apiUrl = _apiUrl + $"/api/Service/{service.Id}";
            var content = new StringContent(JsonSerializer.Serialize(service), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to update service.";
        }

        TempData["Error"] = "Unable to edit service. Please try again.";
        return View(service);
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
