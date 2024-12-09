using Microsoft.AspNetCore.Mvc;
using PosClient.Services;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PosClient.Controllers
{
    public class TaxController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly IUserSessionService _userSessionService;


        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

        public TaxController(HttpClient httpClient, IUserSessionService userSessionService)
        {
            _httpClient = httpClient;
            _userSessionService = userSessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string token = Request.Cookies["authToken"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_apiUrl + "/api/Tax");
            if (response.IsSuccessStatusCode)
            {
                var taxData = await response.Content.ReadAsStringAsync();
                var taxes = JsonSerializer.Deserialize<List<Tax>>(taxData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(taxes);
            }

            TempData["Error"] = "Unable to fetch taxes. Please try again.";
            return View(new List<Tax>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaxDTO tax)
        {
            string token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(tax), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl + $"/api/Tax", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to create tax. See if tax with same category does not already exist.\n";
            return View(tax);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_apiUrl + $"/api/Tax/{id}");
            if (response.IsSuccessStatusCode)
            {
                var taxData = await response.Content.ReadAsStringAsync();
                var tax = JsonSerializer.Deserialize<Tax>(taxData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TaxDTO taxDto = new TaxDTO()
                {
                    Amount = tax.Amount,
                    Name = tax.Name,
                    IsPercentage = tax.IsPercentage,
                    Category = tax.Category
                };

                return View(taxDto);
            }

            TempData["Error"] = "Failed to fetch tax. Please try again.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TaxDTO tax)
        {
            string token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(tax), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(_apiUrl + $"/api/Tax/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update tax. Please try again.";
            return View(tax);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            string token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync(_apiUrl + $"/api/Tax/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to delete tax. Please try again.";
            return RedirectToAction("Index");
        }



    }
}
