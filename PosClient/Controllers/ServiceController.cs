using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using PosClient.Services;
using PosShared;
using PosShared.Models;
using System.Text.Json;
using System.Text;
using PosShared.DTOs;

namespace PosClient.Controllers;

public class ServiceController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IUserSessionService _userSessionService;
    
    private readonly string _apiUrl = UrlConstants.ApiBaseUrl;
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
    public IActionResult Create()
    {
        return View(new ServiceDTO());
    }

    // POST: Service/Create
    [HttpPost]
    public async Task<IActionResult> Create(ServiceDTO serviceDTO)
    {
        if (!ModelState.IsValid)
        {
            return View(serviceDTO);
        }

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + "/api/Service";
        var content = new StringContent(JsonSerializer.Serialize(serviceDTO), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating service: {errorMessage}");

        Console.WriteLine(errorMessage);
        TempData["Error"] = errorMessage;
        return View(serviceDTO);
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
            var service = JsonSerializer.Deserialize<Service>(serviceData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (service != null)
            {
                var serviceDTO = new ServiceDTO
                {
                    Name = service.Name,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    DurationInMinutes = service.DurationInMinutes
                };
                return View(serviceDTO);
            }
        }

        TempData["Error"] = "Unable to fetch service details. Please try again.";
        return RedirectToAction("Index");
    }

    // POST: Service/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ServiceDTO serviceDTO)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var apiUrl = _apiUrl + $"/api/Service/{id}";
            var content = new StringContent(JsonSerializer.Serialize(serviceDTO), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to update service.";
        }

        TempData["Error"] = "Unable to edit service. Please try again.";
        return View(serviceDTO);
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
