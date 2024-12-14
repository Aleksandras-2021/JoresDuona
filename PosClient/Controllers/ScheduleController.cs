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

        public async Task<IActionResult> Schedules(int userId, DateTime? date = null)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
            // Calculate the week dates
            var currentDate = date ?? DateTime.Today;
            var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);
        
            Console.WriteLine($"Getting schedules for week: {weekStart:yyyy-MM-dd} to {weekEnd:yyyy-MM-dd}");
        
            // Get the schedules for the specific week
            var schedulesApiUrl = $"{_apiUrl}/api/Schedule/{userId}/User?startDate={weekStart:yyyy-MM-dd}&endDate={weekEnd:yyyy-MM-dd}";
            var schedulesResponse = await _httpClient.GetAsync(schedulesApiUrl);
        
            List<Schedule> schedules;
            if (schedulesResponse.IsSuccessStatusCode)
            {
                var schedulesJson = await schedulesResponse.Content.ReadAsStringAsync();
                schedules = JsonSerializer.Deserialize<List<Schedule>>(schedulesJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Schedule>();
            }
            else
            {
                schedules = new List<Schedule>();
            }
        
            // Set ViewBag data for the view
            ViewBag.UserId = userId;
            ViewBag.CurrentDate = currentDate;
            ViewBag.WeekStart = weekStart;
            ViewBag.WeekEnd = weekEnd;
        
            return View("~/Views/User/Schedule/Schedules.cshtml", schedules);
        }


        // GET: Schedule/Create
        public IActionResult Create(int userId)
        {
            Console.WriteLine("Creating schedule for user: " + userId);
            var model = new ScheduleCreateViewModel
            {
                UserId = userId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            return View("~/Views/User/Schedule/Create.cshtml", model);
        }

        // POST: Schedule/Create
        [HttpPost]
        public async Task<IActionResult> Create(ScheduleCreateViewModel model)
        {
            Console.WriteLine("Creating schedule: " + JsonSerializer.Serialize(model));
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid");
                return View("~/Views/User/Schedule/Create.cshtml", model);
            }

            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Schedule schedule = new Schedule
            {
                UserId = model.UserId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LastUpdate = DateTime.UtcNow
            };

            var apiUrl = $"{_apiUrl}/api/Schedule";
            var content = new StringContent(
                JsonSerializer.Serialize(schedule), 
                Encoding.UTF8, 
                "application/json");
            

            var response = await _httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Schedules", new { userId = model.UserId });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error creating schedule: {errorMessage}");
            TempData["Error"] = errorMessage;
            return View("~/Views/User/Schedule/Create.cshtml", model);
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
                    var viewModel = new ScheduleCreateViewModel
                    {
                        UserId = schedule.UserId,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime
                    };
                    return View(viewModel);
                }
            }

            return NotFound();
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ScheduleCreateViewModel viewModel)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var schedule = new Schedule
                {
                    Id = id,
                    UserId = viewModel.UserId,
                    StartTime = viewModel.StartTime,
                    EndTime = viewModel.EndTime,
                    LastUpdate = DateTime.UtcNow
                };

                var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
                var content = new StringContent(
                    JsonSerializer.Serialize(schedule), 
                    Encoding.UTF8, 
                    "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Schedules", new { userId = schedule.UserId });
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to update schedule: {errorMessage}");
            }

            return View(viewModel);
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
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Schedule/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Schedules", new { userId });
            }

            TempData["Error"] = "Failed to delete schedule.";
            return RedirectToAction("Schedules", new { userId });
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
                                    User = s.User
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