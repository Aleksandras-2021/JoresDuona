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

            var userResponse = await _httpClient.GetAsync($"{_apiUrl}/api/Users/{userId}");
            string? userName = null;
            if (userResponse.IsSuccessStatusCode)
            {
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(userJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                userName = user?.Name;
            }

            var currentDate = date ?? DateTime.Today;
            var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

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

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.CurrentDate = currentDate;
            ViewBag.WeekStart = weekStart;
            ViewBag.WeekEnd = weekEnd;

            return View("~/Views/User/Schedule/Schedules.cshtml", schedules);
        }


        // GET: Schedule/Create
        public IActionResult Create(int userId)
        {
            var model = new ScheduleCreateViewModel
            {
                UserId = userId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            return View("~/Views/User/Schedule/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduleCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/User/Schedule/Create.cshtml", model);
            }

            if (model.EndTime <= model.StartTime)
            {
                TempData["Error"] = "End time must be after start time.";
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

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["Error"] = "Failed to create schedule. Please try again.";
            return View("~/Views/User/Schedule/Create.cshtml", model);
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