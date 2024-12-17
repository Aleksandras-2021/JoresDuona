using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PosShared.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PosShared.Models; 

public class DiscountsController : Controller
{
    private readonly HttpClient _httpClient;

    public DiscountsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5000/api/"); // PosAPI Base URL

        // Retrieve the token from session and add it to the HTTP header
        var token = httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // GET: Display all discounts
    public async Task<IActionResult> Index()
    {
        try
        {
            var discounts = await _httpClient.GetFromJsonAsync<List<DiscountDto>>("Discounts");

            if (discounts == null || !discounts.Any())
            {
                TempData["Error"] = "No discounts available.";
            }

            return View(discounts ?? new List<DiscountDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to load discounts: {ex.Message}";
            return View(new List<DiscountDto>());
        }
    }



    // GET: Display create form
    public async Task<IActionResult> Create()
    {
        await LoadBusinesses();  // Call method to load businesses into ViewBag
        return View(new DiscountDto());
    }


    // POST: Create a new discount
    [HttpPost]
    public async Task<IActionResult> Create(DiscountDto discountDto)
    {
        ViewBag.DebugInfo = "Create Action Started\n";

        if (!ModelState.IsValid)
        {
            ViewBag.DebugInfo += "ModelState is invalid.\n";
            TempData["Error"] = "Invalid input data.";
            return View(discountDto);
        }

        try
        {
            ViewBag.DebugInfo += "Sending POST request to API...\n";
            ViewBag.DebugInfo += $"Payload: {JsonSerializer.Serialize(discountDto)}\n";

            var response = await _httpClient.PostAsJsonAsync("Discounts", discountDto);

            ViewBag.DebugInfo += $"API Response Status Code: {response.StatusCode}\n";
            ViewBag.DebugInfo += $"Reason Phrase: {response.ReasonPhrase}\n";

            // Log response body for debugging
            var responseBody = await response.Content.ReadAsStringAsync();
            ViewBag.DebugInfo += $"Response Body: {responseBody}\n";

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = $"Failed to create discount: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            ViewBag.DebugInfo += $"Exception: {ex.Message}\n";
            ViewBag.DebugInfo += $"Stack Trace:\n{ex.StackTrace}\n";
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return View(discountDto);
    }


    // GET: Display edit form
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var discount = await _httpClient.GetFromJsonAsync<DiscountDto>($"Discounts/{id}");
            if (discount == null)
            {
                TempData["Error"] = "Discount not found.";
                return RedirectToAction("Index");
            }
            return View(discount);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Update discount
    [HttpPost]
    public async Task<IActionResult> Edit(DiscountDto discountDto)
    {
        ViewBag.DebugInfo = "Edit Action Started";

        if (!ModelState.IsValid)
        {
            ViewBag.DebugInfo += "\nModelState is invalid.";
            TempData["Error"] = "Invalid input data.";
            return View(discountDto);
        }

        try
        {
            ViewBag.DebugInfo += $"\nSending PUT request to API for ID: {discountDto.Id}";

            // Check request payload for debugging
            ViewBag.DebugInfo += $"\nPayload: {JsonSerializer.Serialize(discountDto)}";

            var response = await _httpClient.PutAsJsonAsync($"Discounts/{discountDto.Id}", discountDto);

            ViewBag.DebugInfo += $"\nAPI Response Status Code: {response.StatusCode}";

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.DebugInfo += $"\nAPI call failed: {response.ReasonPhrase}";
            TempData["Error"] = $"Failed to update discount: {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            ViewBag.DebugInfo += $"\nException: {ex.Message}";
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return View(discountDto);
    }

    // POST: Delete discount
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"Discounts/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount deleted successfully.";
            }
            else
            {
                TempData["Error"] = $"Failed to delete discount: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    private async Task LoadBusinesses()
    {
        try
        {
            // Fetch businesses from the API
            var businesses = await _httpClient.GetFromJsonAsync<List<Business>>("Businesses/list");

            if (businesses == null || !businesses.Any())
            {
                TempData["Error"] = "No businesses found.";
                ViewBag.Businesses = new List<Business>();
            }
            else
            {
                ViewBag.Businesses = businesses;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error fetching businesses: {ex.Message}";
            ViewBag.Businesses = new List<Business>();
        }
    }
 



}
