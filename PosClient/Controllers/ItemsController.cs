using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;
using PosShared;
using System.Text;
using PosShared.ViewModels;
using PosShared.DTOs;
using PosClient.Services;

namespace PosClient.Controllers;

public class ItemsController : Controller
{
    private readonly ApiService _apiService;

    public ItemsController(ApiService apiService)
    {
        _apiService = apiService;
    }
    
    // GET: Items/Index
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
    {
        var apiUrl = ApiRoutes.Items.GetItemsPaginated(pageNumber,pageSize);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<PaginatedResult<Item>>(jsonData, JsonOptions.Default);
            return View(items);
        }

        TempData["Error"] = "Could not retrieve items.";
        return View(new PaginatedResult<Item>());
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
        if (item == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid item data.");
            return View(item);
        }

        var apiUrl = ApiRoutes.Items.CreateItem;
        var itemJson = JsonSerializer.Serialize(item);
        var content = new StringContent(itemJson, Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating item: {errorMessage}");

        return View(item);
    }

    // GET: Items/Edit/
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var apiUrl = ApiRoutes.Items.GetItemById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var itemData = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(itemData, JsonOptions.Default);

            if (item != null)
            {
                var itemViewModel = new ItemViewModel
                {
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    BasePrice = item.BasePrice,
                    Category = item.Category,
                    Quantity = item.Quantity
                };

                return View(itemViewModel);
            }
        }

        TempData["Error"] = "Unable to fetch item details. Please try again.";
        return RedirectToAction("Index");
    }


    // POST: Items/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ItemViewModel item)
    {
        
        if (ModelState.IsValid)
        {
            var apiUrl = ApiRoutes.Items.UpdateItem(id);
            var content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");

            var response = await _apiService.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update item.";
        }

        return View(item);
    }

    // GET: Items/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var apiUrl = ApiRoutes.Items.GetItemById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var itemData = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(itemData, JsonOptions.Default);

            if (item != null)
            {
                return View(item);
            }
        }
        TempData["Error"] = "Could not delete item, if its part of an order, it cannot be deleted";

        return NotFound();
    }

    // POST: Items/Delete/{id}
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var apiUrl = ApiRoutes.Items.DeleteItem(id);
        var response = await _apiService.DeleteAsync(apiUrl);

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
        ViewBag.ItemId = itemId; // Pass ItemId to the view

        var apiUrl = ApiRoutes.Items.GetItemVariations(itemId);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var variations = JsonSerializer.Deserialize<List<ItemVariation>>(jsonData,JsonOptions.Default);

            return View("Variations/Variations", variations);
        }

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
        
        var apiUrl = ApiRoutes.Items.CreateVariation(model.ItemId);

        var variation = new ItemVariation()
        {
            Name = model.Name,
            AdditionalPrice = model.AdditionalPrice,
            ItemId = model.ItemId,
        };

        var variationJson = JsonSerializer.Serialize(variation);
        var content = new StringContent(variationJson, Encoding.UTF8, "application/json");
        Console.WriteLine(variationJson);
        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Variations", new { itemId = model.ItemId });
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating variation: {errorMessage}");
        TempData["Error"] = errorMessage;
        return View("Variations/Create", model);
    }



    // GET: Items/Variations/Edit/
    [HttpGet]
    public async Task<IActionResult> EditVariation(int id)
    {
        var apiUrl = ApiRoutes.Items.GetItemVariationById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var variationData = await response.Content.ReadAsStringAsync();

            VariationsDTO? variation = JsonSerializer.Deserialize<VariationsDTO>(variationData, JsonOptions.Default);

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
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";

            return View("Variations/Edit", variation);
        }

        var apiUrl = ApiRoutes.Items.UpdateVariation(id);
        var content = new StringContent(JsonSerializer.Serialize(variation), Encoding.UTF8, "application/json");

        var response = await _apiService.PutAsync(apiUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to update the variation.";
            return View("Variations/Edit", variation);
        }

        return RedirectToAction("Variations", new { itemId = variation.ItemId });
    }




    // POST: Items/Variations/Delete/
    [HttpPost]
    public async Task<IActionResult> DeleteVariation([FromForm] int id, [FromForm] int varId)
    {
        var apiUrl = ApiRoutes.Items.DeleteItem(id);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Variations", new { itemId = id });
        }

        TempData["Error"] = "Failed to delete variation.";
        return RedirectToAction("Variations", new { itemId = id });
    }
    #endregion

}
