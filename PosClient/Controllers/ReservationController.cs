using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosClient.Models;
using PosShared;
using PosShared.Models;
using PosShared.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PosClient.Controllers
{
    public class ReservationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

        public ReservationController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

            // GET: Reservation
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_apiUrl}/api/Reservation");
            
            if (response.IsSuccessStatusCode)
            {
                var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>();
                return View(reservations ?? new List<Reservation>());
            }

            TempData["Error"] = "Could not load reservations.";
            return View(new List<Reservation>());
        }
        
        // GET: Reservation/Reserve/5
        [HttpGet]
        public async Task<IActionResult> Reserve(int serviceId)
        {
            Console.WriteLine($"Attempting to reserve service {serviceId}");
            string? token = Request.Cookies["authToken"];
            Console.WriteLine($"Token: {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            var response = await _httpClient.GetAsync(
                $"{_apiUrl}/api/Reservation/services/{serviceId}/available-slots?date={DateTime.Today:yyyy-MM-dd}");

            if (response.IsSuccessStatusCode)
            {
                var slots = await response.Content.ReadFromJsonAsync<List<TimeSlot>>();
                
                var viewModel = new ReservationViewModel
                {
                    ServiceId = serviceId,
                    AvailableTimeSlots = slots ?? new List<TimeSlot>()
                };
                
                return View("~/Views/Service/Reserve.cshtml", viewModel);
            }

            TempData["Error"] = "Could not load available time slots.";
            return RedirectToAction("Index", "Service");
        }
    }
}