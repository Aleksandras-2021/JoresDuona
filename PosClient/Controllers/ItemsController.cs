using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using PosShared;
using PosShared.Ultilities;
using System.Text;
using PosShared.ViewModels;

namespace PosClient.Controllers;

public class ItemsController : Controller
{
    private readonly HttpClient _httpClient;

    private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

    public ItemsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // GET: Items/Index
    public async Task<IActionResult> Index()
    {

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine(Ultilities.ExtractUserRoleFromToken(token));

        var apiUrl = _apiUrl + "/api/Items";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<Item>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(items);
        }

        // Handle errors or empty results
        ViewBag.ErrorMessage = "Could not retrieve users.";
        return View(new List<Item>());
    }


    // GET: Items/Create
    public IActionResult Create()
    {
        return View(new ItemCreateViewModel());
    }

    // POST: Items/Create
    [HttpPost]
    public async Task<IActionResult> Create(ItemCreateViewModel item)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);



        if (item == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid item data.");
            return View(item);
        }

        Console.WriteLine(JsonSerializer.Serialize(item).ToString());

        var apiUrl = _apiUrl + "/api/Items";
        var itemJson = JsonSerializer.Serialize(item);
        var content = new StringContent(itemJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Redirect to index page after successful creation
            return RedirectToAction(nameof(Index));
        }

        // Handle errors
        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating item: {errorMessage}");
        return View(item);
    }

    // GET: Items/Edit/
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Items/{id}";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string itemData = await response.Content.ReadAsStringAsync();

            Item item = JsonSerializer.Deserialize<Item>(itemData);
            ItemCreateViewModel itemViewModel = new ItemCreateViewModel();

            itemViewModel.Price = item.Price;
            itemViewModel.BasePrice = item.Price;
            itemViewModel.Name = item.Name;
            itemViewModel.Description = item.Description;
            itemViewModel.Quantity = item.Quantity;

            if (itemViewModel != null)
            {
                return View(itemViewModel);
            }
        }

        return NotFound();
    }

    // POST: Items/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ItemCreateViewModel item)
    {

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var apiUrl = _apiUrl + $"/api/Items/{id}";
            var content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to update user.";
        }

        return View(item); // Return to the edit view if validation fails or update fails
    }

}
