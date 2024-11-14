using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

namespace PosClient.Controllers
{
    public class BusinessController : Controller
    {
        private readonly HttpClient _httpClient;

        public BusinessController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Business/Index
        public async Task<IActionResult> Index()
        {
            var apiUrl = "http://localhost:5149/api/Businesses";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var businesses = JsonSerializer.Deserialize<List<Business>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(businesses);
            }

            // Handle errors or empty results
            ViewBag.ErrorMessage = "Could not retrieve businesses.";
            return View(new List<Business>());
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
            if (ModelState.IsValid)
            {
                var apiUrl = "http://localhost:5149/api/Businesses";
                var content = new StringContent(JsonSerializer.Serialize(business), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                // Handle error response
                ViewBag.ErrorMessage = "Failed to create business.";
            }

            return View(business);
        }

        // Additional actions for Edit, Details, and Delete can be added similarly

        // GET: Business/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Businesses/{id}";
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

            return NotFound(); // Return a 404 if the business was not found or request failed
        }

        // POST: Business/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Business business)
        {
            if (id != business.Id)
            {
                return BadRequest("Business ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                var apiUrl = $"http://localhost:5149/api/Businesses/{id}";
                var content = new StringContent(JsonSerializer.Serialize(business), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to the Index action after successful edit
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to update business.";
            }

            return View(business); // Return to the edit view if validation fails or update fails
        }

        // GET: Business/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Businesses/{id}";
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

            return NotFound(); // Return a 404 if the business was not found or request failed
        }

        // POST: Business/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Businesses/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the Index action after successful deletion
                return RedirectToAction("Index");
            }

            // Return an error message if the delete failed
            ViewBag.ErrorMessage = "Failed to delete business.";
            return RedirectToAction("Index");
        }


    }



}
