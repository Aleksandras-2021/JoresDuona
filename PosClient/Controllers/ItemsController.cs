using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using PosShared;
using PosShared.Ultilities;
using System.Text;

namespace PosClient.Controllers
{
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
            return View(new Item());
        }

        // POST: Items/Create
        [HttpPost]
        public async Task<IActionResult> Create(Item item)
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

    }
}
