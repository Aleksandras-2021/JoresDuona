using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json; 

public class DiscountsController : Controller
{
    private readonly HttpClient _client;

    public DiscountsController()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5000/")
        };
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var discounts = await _client.GetFromJsonAsync<List<Discount>>("api/Discounts");
            return View(discounts);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to load discounts: {ex.Message}";
            Console.WriteLine($"Error in Index: {ex.Message}");
            return View(new List<Discount>());
        }
    }

    public IActionResult Create()
    {
        return View(new Discount
        {
            ValidFrom = DateTime.Now,
            ValidTo = DateTime.Now.AddDays(7) // Use DateTime directly
        });
    }
    [HttpPost]
    public async Task<IActionResult> Create(Discount discount)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid data.";
            return View(discount);
        }

        discount.BusinessId = 1; // Ensure BusinessId is set here

        discount.ValidFrom = DateTime.SpecifyKind(discount.ValidFrom, DateTimeKind.Utc);
        discount.ValidTo = DateTime.SpecifyKind(discount.ValidTo, DateTimeKind.Utc);

        try
        {
            var response = await _client.PostAsJsonAsync("api/Discounts", discount);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount created successfully.";
                return RedirectToAction("Index");
            }
            var apiError = await response.Content.ReadAsStringAsync();
            TempData["Error"] = "Failed to create the discount. " + apiError;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return View(discount);
    }


    public async Task<IActionResult> Edit(int id)
    {
        Console.WriteLine($"Debug: Edit method called for ID {id}");
        try
        {
            var discount = await _client.GetFromJsonAsync<Discount>($"api/Discounts/{id}");
            Console.WriteLine($"Fetched Discount: {JsonSerializer.Serialize(discount)}");
            return View(discount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching discount for Edit: {ex.Message}");
            TempData["Error"] = "Failed to load discount.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Discount discount)
    {
        if (id != discount.Id)
        {
            TempData["Error"] = "ID mismatch.";
            return View(discount);
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid data.";
            return View(discount);
        }

        discount.BusinessId = 1; // Ensure BusinessId is valid

        discount.ValidFrom = DateTime.SpecifyKind(discount.ValidFrom, DateTimeKind.Utc);
        discount.ValidTo = DateTime.SpecifyKind(discount.ValidTo, DateTimeKind.Utc);

        try
        {
            var response = await _client.PutAsJsonAsync($"api/Discounts/{id}", discount);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount updated successfully.";
                return RedirectToAction("Index");
            }
            var apiError = await response.Content.ReadAsStringAsync();
            TempData["Error"] = "Failed to edit the discount. " + apiError;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return View(discount);
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        Console.WriteLine($"Debug: Delete method called for ID {id}");
        try
        {
            var response = await _client.DeleteAsync($"api/Discounts/{id}");
            Console.WriteLine($"Response Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Discount deleted successfully.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error Response: {errorMessage}");
                TempData["Error"] = "Failed to delete the discount. " + errorMessage;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in Delete: {ex.Message}");
            TempData["Error"] = $"An error occurred: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [HttpGet("activate/{name}")]
    public async Task<IActionResult> ActivateDiscount(string name)
    {
        Console.WriteLine($"Debug: ActivateDiscount called for Name {name}");
        try
        {
            var response = await _client.GetAsync($"api/Discounts/activate/{name}");
            Console.WriteLine($"Response Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error Response: {errorMessage}");
                TempData["Error"] = "Failed to activate discount. " + errorMessage;
                return RedirectToAction("Index");
            }

            var discount = await response.Content.ReadFromJsonAsync<Discount>();
            Console.WriteLine($"Activated Discount: {JsonSerializer.Serialize(discount)}");
            TempData["Success"] = $"Discount '{discount.Description}' activated successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ActivateDiscount: {ex.Message}");
            TempData["Error"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}
