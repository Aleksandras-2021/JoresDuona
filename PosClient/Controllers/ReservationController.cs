using Microsoft.AspNetCore.Mvc;
using PosShared;
using PosShared.Models;
using PosShared.ViewModels;
using System.Text;
using System.Text.Json;
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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var reservations = await GetAllReservations(pageNumber,pageSize);
            return View(reservations);
        }

        // GET: Reservation/Reserve/5
        [HttpGet]
        public async Task <IActionResult> Reserve(int serviceId,int pageNumber = 1, int pageSize = 10)
        {
            var reservations = await GetAllReservations(1,10);
            var activeReservations = reservations.Items
                .Where(i => i.ReservationEndTime > DateTime.Now)
                .ToList();

            reservations.Items = activeReservations;
            
            ReservationViewModel model = new ReservationViewModel()
            {
                ServiceId = serviceId,
                CustomerName = string.Empty,
                CustomerPhone = string.Empty,
                ReservationTime = DateTime.Now,
                Reservations = reservations,
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
                ReservationTime = model.ReservationTime.ToUniversalTime()
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

            return RedirectToAction(nameof(Reserve), new { serviceId = model.ServiceId });
        }
        
        
        // GET: Reservation/Reserve/5
        [HttpGet]
        public async Task<IActionResult>  Edit(int reservationId)
        {
            var apiUrl = ApiRoutes.Reservation.GetById(reservationId);
            var response = await _apiService.GetAsync(apiUrl);
        
            
            if (response.IsSuccessStatusCode)
            {
                var reservation = await response.Content.ReadFromJsonAsync<Reservation>();
                
                 var model = new ReservationViewModel()
                {
                    Id = reservationId,
                    ServiceId = reservation.ServiceId,
                    CustomerName = reservation.CustomerName,
                    CustomerPhone = reservation.CustomerPhone,
                    ReservationTime = reservation.ReservationTime.ToLocalTime()
                };
                return View("~/Views/Reservation/Edit.cshtml", model);
            }
            
            return RedirectToAction("Index");
        }

        
        // GET: Reservation/Reserve/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id,ReservationViewModel model)
        {
            var dto = new ReservationCreateDTO()
            {
                ServiceId = model.ServiceId,
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                ReservationTime = model.ReservationTime.ToUniversalTime(),
            };
            Console.WriteLine($"Received ID: {id}"); // Debugging

            
            var apiUrl = ApiRoutes.Reservation.Update(id);
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            
            var response = await _apiService.PutAsync(apiUrl,content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"Reservation Updated. {response.StatusCode}";
                return RedirectToAction("Index");
            }

            TempData["Error"] = $"Could not update reservation. {response.StatusCode}";

            return RedirectToAction("Index","Reservation");
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


        private async Task<PaginatedResult<Reservation>> GetAllReservations(int pageNumber,int pageSize)
        {
            var apiUrl = ApiRoutes.Reservation.ListPaginated(pageNumber,pageSize);
            var response = await _apiService.GetAsync(apiUrl);
        
            if (response.IsSuccessStatusCode)
            {
                var reservations = await response.Content.ReadFromJsonAsync<PaginatedResult<Reservation>>();
                return (reservations ?? new PaginatedResult<Reservation>());
            }
            TempData["Error"] = "Error response status: " + response.StatusCode;
            return (new PaginatedResult<Reservation>());
        }
        
        
    }
}
