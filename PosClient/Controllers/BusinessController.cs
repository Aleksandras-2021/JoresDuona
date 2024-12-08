﻿using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using PosShared;
using PosShared.Ultilities;
using System.Net.Http.Headers;

namespace PosClient.Controllers
{
    public class BusinessController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;
        private readonly ILogger<BusinessController> _logger;


        public BusinessController(HttpClient httpClient, ILogger<BusinessController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET: Business/Index (retrieves all businesses)
        public async Task<IActionResult> Index()
        {
            string? token = Request.Cookies["authToken"]; // Retrieve the token from cookies

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Put the token into authroization header

            var apiUrl = _apiUrl + "/api/Businesses";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                List<Business> businesses = JsonSerializer.Deserialize<List<Business>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = _apiUrl + "/api/Businesses";
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

        // GET: Business/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/Businesses/{id}";
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
                string? token = Request.Cookies["authToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _apiUrl + $"/api/Businesses/{id}";
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
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/Businesses/{id}";
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
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/Businesses/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the Index action after successful deletion
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to delete business.";
            return RedirectToAction("Index");
        }


    }



}
