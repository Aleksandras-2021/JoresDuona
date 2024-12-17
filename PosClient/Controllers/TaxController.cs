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

        private readonly ApiService _apiService;

        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

        public TaxController(ApiService apiService, IUserSessionService userSessionService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _apiService.GetAsync(_apiUrl + "/api/Tax");
            if (response.IsSuccessStatusCode)
            {
                var taxData = await response.Content.ReadAsStringAsync();
                var taxes = JsonSerializer.Deserialize<List<Tax>>(taxData, JsonOptions.Default);
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
            var content = new StringContent(JsonSerializer.Serialize(tax), Encoding.UTF8, "application/json");
            var response = await _apiService.PostAsync(_apiUrl + $"/api/Tax", content);

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
            var response = await _apiService.GetAsync(_apiUrl + $"/api/Tax/{id}");
            if (response.IsSuccessStatusCode)
            {
                var taxData = await response.Content.ReadAsStringAsync();
                var tax = JsonSerializer.Deserialize<Tax>(taxData, JsonOptions.Default);
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
            var content = new StringContent(JsonSerializer.Serialize(tax), Encoding.UTF8, "application/json");

            var response = await _apiService.PutAsync(_apiUrl + $"/api/Tax/{id}", content);

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
            var response = await _apiService.DeleteAsync(_apiUrl + $"/api/Tax/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to delete tax. Please try again.";
            return RedirectToAction("Index");
        }
        
    }
}
