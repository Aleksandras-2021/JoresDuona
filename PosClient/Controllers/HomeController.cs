using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;

namespace PosClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        // Inject HttpClient via constructor
        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Home/Index
        public ActionResult Index()
        {
            return View();
        }

        // POST: Home/GetBusinessById
        [HttpPost]
        public async Task<IActionResult> GetBusinessById(int businessId)
        {
            var apiUrl = $"http://localhost:5149/api/Businesses/{businessId}";
            var response = await _httpClient.GetAsync(apiUrl);

            Console.WriteLine(response.ToString());

            if (response.IsSuccessStatusCode)
            {
                var businessJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content: " + businessJson); // Add this line to see what the content looks like

                // Deserialize the JSON response into a Business object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Optionally add this to ignore case in property names
                };

                var business = JsonSerializer.Deserialize<Business>(businessJson, options);

                // Store the deserialized business object in TempData (serialize it back to string)
                TempData["BusinessData"] = JsonSerializer.Serialize(business);
                return RedirectToAction("Result");
            }

            TempData["BusinessData"] = "Business not found or an error occurred.";
            return RedirectToAction("Result");
        }

        // GET: Home/Result
        public IActionResult Result()
        {
            var jsonData = TempData["BusinessData"]?.ToString();
            Business? business = null;

            if (!string.IsNullOrEmpty(jsonData))
            {
                // Deserialize the business data back into a Business object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Optional: case insensitive deserialization
                };
                business = JsonSerializer.Deserialize<Business>(jsonData, options);
            }

            return View(business); // Pass the object to the view
        }
    }
}
