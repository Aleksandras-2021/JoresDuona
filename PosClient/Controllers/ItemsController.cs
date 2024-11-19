using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using PosShared;
using PosShared.Ultilities;

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
    }
}
