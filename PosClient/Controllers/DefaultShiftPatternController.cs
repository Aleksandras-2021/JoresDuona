using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using PosShared;
using System.Text;

namespace PosClient.Controllers
{
    public class DefaultShiftPatternController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

        public DefaultShiftPatternController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> ShiftPatterns()
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_apiUrl + "/api/DefaultShiftPattern");

            if (response.IsSuccessStatusCode)
            {
                var patternsData = await response.Content.ReadAsStringAsync();
                var patterns = JsonSerializer.Deserialize<List<DefaultShiftPattern>>(
                    patternsData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                return View(patterns);
            }

            TempData["Error"] = "Unable to fetch shift patterns. Please try again.";
            return View(new List<DefaultShiftPattern>());
        }

        // POST: DefaultShiftPattern/Create
        [HttpPost]
        public async Task<IActionResult> Create(DefaultShiftPattern pattern)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = _apiUrl + "/api/DefaultShiftPattern";
                var content = new StringContent(JsonSerializer.Serialize(pattern), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(ShiftPatterns));
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error creating shift pattern: {errorMessage}");
            }

            return View(pattern);
        }

        // GET: DefaultShiftPattern/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var patternData = await response.Content.ReadAsStringAsync();
                var pattern = JsonSerializer.Deserialize<DefaultShiftPattern>(patternData, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pattern != null)
                {
                    return View(pattern);
                }
            }

            return NotFound();
        }

        // POST: DefaultShiftPattern/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, DefaultShiftPattern pattern)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}";
                var content = new StringContent(JsonSerializer.Serialize(pattern), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(ShiftPatterns));
                }

                TempData["Error"] = "Failed to update shift pattern.";
            }

            return View(pattern);
        }

        // POST: DefaultShiftPattern/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(ShiftPatterns));
            }

            TempData["Error"] = "Could not delete the shift pattern.";
            return RedirectToAction(nameof(ShiftPatterns));
        }
    }
}
