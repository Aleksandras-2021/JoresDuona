using Microsoft.AspNetCore.Mvc;
using PosShared;
using PosShared.Models;
using PosShared.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosClient.Services;
using PosShared.DTOs;

namespace PosClient.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApiService _apiService;

        public ReservationController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Reservation
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apiUrl = ApiRoutes.Reservation.Get;
            var response = await _apiService.GetAsync(apiUrl);
        
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
        public IActionResult Reserve(int serviceId)
        {
            ReservationViewModel model = new ReservationViewModel()
            {
                ServiceId = serviceId,
                CustomerName = string.Empty,
                CustomerPhone = string.Empty,
                ReservationTime = DateTime.Now
            };
            
            return View("~/Views/Service/Reserve.cshtml", model);
        }

        
        // Post: Reservation/Reserve/5
        [HttpPost]
        public async Task<IActionResult> Reserve(ReservationViewModel model)
        {
            var dto = new ReservationCreateDTO()
            {
                ServiceId = model.ServiceId,
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                ReservationTime = model.ReservationTime,
            };
            
            
            var apiUrl = ApiRoutes.Reservation.Create;
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            
            var response = await _apiService.PostAsync(apiUrl,content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"Reservation created. {response.StatusCode}";
               return RedirectToAction("Index");
            }

            TempData["Error"] = $"Could not Create reservation. {response.StatusCode}";

            return View("~/Views/Service/Reserve.cshtml", model);
        }
        
        
        [HttpPost]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            var apiUrl = ApiRoutes.Reservation.Delete(reservationId);
            var response = await _apiService.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"Reservation Canceled. {response.StatusCode}";
                return RedirectToAction("Index");
            }
            TempData["Error"] = $"Could not delete reservation. {response.StatusCode}";

            return RedirectToAction("Index");
        }
    }
}
