using Microsoft.AspNetCore.Mvc;
using PosClient.Services;
using PosShared;
using PosShared.Models;
using PosShared.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PosClient.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IUserSessionService _userSessionService;
        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

        public ScheduleController(HttpClient httpClient, IUserSessionService userSessionService)
        {
            _httpClient = httpClient;
            _userSessionService = userSessionService;
        }

    // GET: Schedule/Schedules
    public async Task<IActionResult> Schedules(int userId)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
        var currentDate = DateTime.Today;
        var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
    
        var apiUrl = $"{_apiUrl}/api/Schedule?userId={userId}&startDate={weekStart:yyyy-MM-dd}&endDate={weekEnd:yyyy-MM-dd}";
        
        var response = await _httpClient.GetAsync(apiUrl);
    
        if (response.IsSuccessStatusCode)
        {
            var schedulesJson = await response.Content.ReadAsStringAsync();
            var schedules = JsonSerializer.Deserialize<List<Schedule>>(schedulesJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View("~/Views/User/Schedule/Schedules.cshtml", schedules);
        }
    
        return View("~/Views/User/Schedule/Schedules.cshtml", new List<Schedule>());
    }

        // GET: Schedule/Create
        public IActionResult Create()
        {
            return View(new Schedule());
        }

        // POST: Schedule/Create
        [HttpPost]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = $"{_apiUrl}/api/Schedule";
                var content = new StringContent(
                    JsonSerializer.Serialize(schedule), 
                    Encoding.UTF8, 
                    "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, 
                    $"Failed to create schedule: {errorMessage}");
            }

            return View(schedule);
        }

        // GET: Schedule/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var scheduleData = await response.Content.ReadAsStringAsync();
                var schedule = JsonSerializer.Deserialize<Schedule>(scheduleData, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (schedule != null)
                {
                    return View(schedule);
                }
            }

            return NotFound();
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
                var content = new StringContent(
                    JsonSerializer.Serialize(schedule), 
                    Encoding.UTF8, 
                    "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, 
                    $"Failed to update schedule: {errorMessage}");
            }

            return View(schedule);
        }

        // GET: Schedule/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var scheduleData = await response.Content.ReadAsStringAsync();
                var schedule = JsonSerializer.Deserialize<Schedule>(scheduleData);

                if (schedule != null)
                {
                    return View(schedule);
                }
            }

            return NotFound();
        }

        // POST: Schedule/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to delete schedule.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Schedule/WeekView
        public async Task<IActionResult> WeekView(DateTime? date = null)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var currentDate = date ?? DateTime.Today;
            var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var apiUrl = $"{_apiUrl}/api/Schedule?startDate={weekStart:yyyy-MM-dd}&endDate={weekEnd:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var schedules = JsonSerializer.Deserialize<List<Schedule>>(jsonData, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var viewModel = new WeeklyScheduleViewModel
                {
                    WeekStartDate = weekStart,
                    WeekEndDate = weekEnd,
                    DailySchedules = Enumerable.Range(0, 7)
                        .Select(offset => new DailyScheduleViewModel
                        {
                            Date = weekStart.AddDays(offset),
                            Schedules = schedules?
                                .Where(s => s.StartTime.Date == weekStart.AddDays(offset).Date)
                                .Select(s => new ScheduleViewModel
                                {
                                    Id = s.Id,
                                    UserId = s.UserId,
                                    UserName = s.User?.Name ?? "Unknown",
                                    StartTime = s.StartTime,
                                    EndTime = s.EndTime
                                })
                                .ToList() ?? new List<ScheduleViewModel>()
                        })
                        .ToList()
                };

                return View(viewModel);
            }

            return View(new WeeklyScheduleViewModel
            {
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                DailySchedules = new List<DailyScheduleViewModel>()
            });
        }
    }
}