using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using PosShared;
using PosShared.Ultilities;
using System.Text;
using PosShared.ViewModels;
using PosShared.DTOs;

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

        var apiUrl = _apiUrl + "/api/Items";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<Item>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(items);
        }

        // Handle errors or empty results
        TempData["Error"] = "Could not retrieve users.";
        return View(new List<Item>());
    }


    // GET: Items/Create
    public IActionResult Create()
    {
        return View(new ItemViewModel());
    }

    // POST: Items/Create
    [HttpPost]
    public async Task<IActionResult> Create(ItemViewModel item)
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
            var itemData = await response.Content.ReadAsStringAsync();
            Console.Write(itemData);

            Item? item = JsonSerializer.Deserialize<Item>(itemData);

            ItemViewModel itemViewModel = new ItemViewModel();

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
    public async Task<IActionResult> Edit(int id, ItemViewModel item)
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

            TempData["Error"] = "Failed to update item.";
        }

        return View(item); // Return to the edit view if validation fails or update fails
    }

    // GET: Items/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Items/{id}";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var itemData = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(itemData);

            if (item != null)
            {
                return View(item);
            }
        }

        return NotFound(); // Return a 404 if the user was not found or request failed
    }

    // POST: Items/Delete/{id}
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Items/{id}";
        var response = await _httpClient.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        ViewBag.ErrorMessage = "Failed to delete item.";
        return RedirectToAction("Index");
    }

    #region Variations

    // GET: Items/Variations/{itemId}
    [HttpGet]
    public async Task<IActionResult> Variations(int itemId)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ViewBag.ItemId = itemId; // Pass ItemId to the view


        // API URL for fetching item variations
        var apiUrl = _apiUrl + $"/api/Items/{itemId}/Variations";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var variations = JsonSerializer.Deserialize<List<ItemVariation>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Pass the variations to the view
            return View("Variations/Variations", variations);
        }

        // Handle errors or empty results
        ViewBag.ErrorMessage = "Could not retrieve variations for the item.";
        return View("Variations/Variations", new List<ItemVariation>());
    }

    // GET: Items/Variations/Create/{itemId}
    [HttpGet]
    public IActionResult VariationCreate(int itemId)
    {
        var model = new ItemVariationCreateViewModel
        {
            ItemId = itemId
        };

        return View("Variations/Create", model);
    }

    // POST: Items/Variations/Create
    [HttpPost]
    public async Task<IActionResult> VariationCreate(ItemVariationCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Variations/Create", model);
        }

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Items/{model.ItemId}/Variations";

        ItemVariation variation = new ItemVariation();
        variation.Name = model.Name;
        variation.AdditionalPrice = model.AdditionalPrice;
        variation.ItemId = model.ItemId;

        var variationJson = JsonSerializer.Serialize(variation);
        var content = new StringContent(variationJson, Encoding.UTF8, "application/json");
        Console.WriteLine(variationJson);
        var response = await _httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Redirect to variations list after successful creation
            return RedirectToAction("Variations", new { itemId = model.ItemId });
        }

        // Handle errors
        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating variation: {errorMessage}");
        TempData["Error"] = errorMessage;
        return View("Variations/Create", model);
    }



    // GET: Items/Variations/Edit/
    [HttpGet]
    public async Task<IActionResult> EditVariation(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Items/Variations/{id}";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var variationData = await response.Content.ReadAsStringAsync();

            VariationsDTO variation = JsonSerializer.Deserialize<VariationsDTO>(variationData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            if (variation != null)
            {
                return View("Variations/Edit", variation);
            }
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EditVariation(int id, VariationsDTO variation)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";

            return View("Variations/Edit", variation);
        }

        // Prepare the API request
        var apiUrl = _apiUrl + $"/api/Items/Variations/{id}";
        var content = new StringContent(JsonSerializer.Serialize(variation), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(apiUrl, content);
        Console.WriteLine(response);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to update the variation.";
            return View("Variations/Edit", variation); // Return the view with the current model if the API fails
        }

        // Redirect to the Variations list on success
        return RedirectToAction("Variations", new { itemId = variation.ItemId });
    }




    // POST: Items/Variations/Delete/
    [HttpPost]
    public async Task<IActionResult> DeleteVariation([FromForm] int id, [FromForm] int varId)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = $"{_apiUrl}/api/Items/Variations/{varId}";
        var response = await _httpClient.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Variations", new { itemId = id });
        }

        TempData["Error"] = "Failed to delete variation.";
        return RedirectToAction("Variations", new { itemId = id });
    }




    #endregion

}
