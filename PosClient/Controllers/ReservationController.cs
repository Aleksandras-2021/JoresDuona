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

public class ReservationController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

    public ReservationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET: Reservations/Reserve/5 (5 is the serviceId)
    public async Task<IActionResult> Reserve(int serviceId)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get available slots from API
        var response = await _httpClient.GetAsync(
            $"{_apiUrl}/api/Reservations/services/{serviceId}/available-slots?date={DateTime.Today:yyyy-MM-dd}");

        if (response.IsSuccessStatusCode)
        {
            var slots = await response.Content.ReadFromJsonAsync<List<TimeSlot>>();
            
            var viewModel = new ReservationViewModel
            {
                ServiceId = serviceId,
                AvailableTimeSlots = slots ?? new List<TimeSlot>()
            };
            
            return View(viewModel);
        }

        TempData["Error"] = "Could not load available time slots.";
        return RedirectToAction("Index", "Service");
    }
}