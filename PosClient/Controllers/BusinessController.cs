using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using PosShared;
using PosShared.Utilities;
using System.Net.Http.Headers;

namespace PosClient.Controllers
{
    public class BusinessController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;
        private readonly ILogger<BusinessController> _logger;


        public BusinessController(HttpClient httpClient, ILogger<BusinessController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET: Business/Index (retrieves all businesses)
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            string? token = Request.Cookies["authToken"]; // Retrieve the token from cookies

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Put the token into authroization header

            var apiUrl = ApiRoutes.Business.GetPaginated(pageNumber, pageSize);
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var paginatedResult = JsonSerializer.Deserialize<PaginatedResult<Business>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                
                return View(paginatedResult);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Error: {errorContent}";
            
            return View(new PaginatedResult<Business>());
        }

        // GET: Business/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Business/Create
        [HttpPost]
        public async Task<IActionResult> Create(Business business)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = ApiRoutes.Business.Create;
                var content = new StringContent(JsonSerializer.Serialize(business), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Failed to create business. " + response.StatusCode;
            }

            return View(business);
        }

        // GET: Business/Edit/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = ApiRoutes.Business.GetById(id);
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var businessData = await response.Content.ReadAsStringAsync();
                var business = JsonSerializer.Deserialize<Business>(businessData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (business != null)
                {
                    return View(business);
                }
            }

            TempData["Error"] = $"Failed to create business: {response.StatusCode}";

            return NotFound();
        }

        // POST: Business/Edit/
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Business business)
        {
            if (id != business.Id)
            {
                return BadRequest("Business ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                string? token = Request.Cookies["authToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = ApiRoutes.Business.Update(id);
                var content = new StringContent(JsonSerializer.Serialize(business), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                Console.WriteLine("Business Id : " + business.Id);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to update business." + response.StatusCode;

            }

            return View(business); 
        }

        // GET: Business/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = ApiRoutes.Business.GetById(1);
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var businessData = await response.Content.ReadAsStringAsync();
                var business = JsonSerializer.Deserialize<Business>(businessData);

                if (business != null)
                {
                    return View(business);
                }
            }

            return NotFound();
        }

        // POST: Business/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = ApiRoutes.Business.Delete(id);
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = $"Failed to delete business. {response.StatusCode}";
            return RedirectToAction("Index");
        }
    }
}
