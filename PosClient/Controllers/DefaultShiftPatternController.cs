using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using PosShared;
using PosShared.ViewModels;
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
                var patterns = JsonSerializer.Deserialize<List<DefaultShiftPattern>>(patternsData, JsonOptions.Default);
                Console.WriteLine("Patterns:" + JsonSerializer.Serialize(patterns));
                return View(patterns);
            }

           TempData["Error"] = "Unable to fetch shift patterns. Please try again.";
           return View(new List<DefaultShiftPattern>());
       }

        // GET: DefaultShiftPattern/Create
        [HttpGet]
        public async Task<IActionResult> Create(int pageNumber = 1, int pageSize = 20)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var usersApiUrl = ApiRoutes.User.GetPaginated(pageNumber, pageSize);
            var userResponse = await _httpClient.GetAsync(usersApiUrl);

            PaginatedResult<User>? users = null;
            if (userResponse.IsSuccessStatusCode)
            {
                var usersJson = await userResponse.Content.ReadAsStringAsync();
                users = JsonSerializer.Deserialize<PaginatedResult<User>>(usersJson, JsonOptions.Default);
            }
            else
            {
                users = new PaginatedResult<User>();
            }

            var model = new DefaultShiftPatternCreateViewModel
            {
                AvailableUsers = users,
                Pattern = new DefaultShiftPattern
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(8)
                }
            };

            return View(model);
        }

        // POST: DefaultShiftPattern/AssignUser
        [HttpPost]
        public async Task<IActionResult> AssignUser(int userId, int patternId)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{_apiUrl}/api/DefaultShiftPattern/{patternId}/User/{userId}", null);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to assign user" });
        }

        // POST: DefaultShiftPattern/RemoveUser
        [HttpPost]
        public async Task<IActionResult> RemoveUser(int userId, int patternId)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"{_apiUrl}/api/DefaultShiftPattern/{patternId}/User/{userId}");

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to remove user" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DefaultShiftPatternCreateViewModel viewModel, List<int> assignedUserIds)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                viewModel.Pattern.StartDate = new DateTime(2000, 1, 1, 
                    viewModel.Pattern.StartDate.Hour, 0, 0, DateTimeKind.Utc);
                viewModel.Pattern.EndDate = new DateTime(2000, 1, 1, 
                    viewModel.Pattern.EndDate.Hour, 0, 0, DateTimeKind.Utc);

                var apiUrl = _apiUrl + "/api/DefaultShiftPattern";
                var content = new StringContent(
                    JsonSerializer.Serialize(viewModel.Pattern),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var createdPattern = await response.Content.ReadFromJsonAsync<DefaultShiftPattern>();
                    if (createdPattern != null && assignedUserIds != null)
                    {
                        foreach (var userId in assignedUserIds)
                        {
                            await AssignUser(userId, createdPattern.Id);
                        }
                    }

                    return RedirectToAction(nameof(ShiftPatterns));
                }
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error creating shift pattern: {errorMessage}");
            }

            var usersApiUrl = ApiRoutes.User.GetPaginated(1, 20);
            var userResponse = await _httpClient.GetAsync(usersApiUrl);
            if (userResponse.IsSuccessStatusCode)
            {
                var usersJson = await userResponse.Content.ReadAsStringAsync();
                viewModel.AvailableUsers = JsonSerializer.Deserialize<PaginatedResult<User>>(
                    usersJson, JsonOptions.Default);
            }

            return View(viewModel);
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
                   JsonOptions.Default);

                if (pattern != null)
                {
                    
                    return View("Create");
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