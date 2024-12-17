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

        public async Task<IActionResult> Index()
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_apiUrl + "/api/DefaultShiftPattern");

            if (response.IsSuccessStatusCode)
            {
                var patternsData = await response.Content.ReadAsStringAsync();
                var patterns = JsonSerializer.Deserialize<List<DefaultShiftPattern>>(patternsData, JsonOptions.Default);
                return View(patterns);
            }

            TempData["Error"] = "Unable to fetch shift patterns. Please try again.";
            return View(new List<DefaultShiftPattern>());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int pageNumber = 1, int pageSize = 20)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var pattern = new DefaultShiftPattern
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(8)
            };

            return View(await ReloadViewModelWithUsers(pattern, pageNumber, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Create(DefaultShiftPatternCreateViewModel viewModel, List<int> assignedUserIds)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.Pattern.EndDate.TimeOfDay <= viewModel.Pattern.StartDate.TimeOfDay)
                    {
                        TempData["Error"] = "End time must be after start time.";
                        return View(await ReloadViewModelWithUsers(viewModel.Pattern));
                    }

                    viewModel.Pattern.StartDate = new DateTime(2000, 1, 1, 
                        viewModel.Pattern.StartDate.Hour, 0, 0, DateTimeKind.Utc);
                    viewModel.Pattern.EndDate = new DateTime(2000, 1, 1, 
                        viewModel.Pattern.EndDate.Hour, 0, 0, DateTimeKind.Utc);

                    var apiUrl = $"{_apiUrl}/api/DefaultShiftPattern?{string.Join("&", assignedUserIds.Select(id => $"userIds={id}"))}";
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

                        return RedirectToAction(nameof(Index));
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = errorMessage;
                    }
                    else
                    {
                        TempData["Error"] = $"Failed to create shift pattern. " + response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(await ReloadViewModelWithUsers(viewModel.Pattern));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int pageNumber = 1, int pageSize = 20)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var patternData = await response.Content.ReadAsStringAsync();
                var pattern = JsonSerializer.Deserialize<DefaultShiftPattern>(patternData, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pattern != null)
                {
                    return View(await ReloadViewModelWithUsers(pattern, pageNumber, pageSize));
                }
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DefaultShiftPattern pattern, List<int> assignedUserIds)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                if (pattern.EndDate.TimeOfDay <= pattern.StartDate.TimeOfDay)
                {
                    TempData["Error"] = "End time must be after start time.";
                    return View(await ReloadViewModelWithUsers(pattern));
                }

                try
                {
                    var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}?{string.Join("&", assignedUserIds.Select(id => $"userIds={id}"))}";
                    var content = new StringContent(
                        JsonSerializer.Serialize(pattern),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.PutAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        if (assignedUserIds != null)
                        {
                            var currentPattern = await _httpClient.GetFromJsonAsync<DefaultShiftPattern>($"{_apiUrl}/api/DefaultShiftPattern/{id}");
                            var currentUserIds = currentPattern?.Users?.Select(u => u.Id).ToList() ?? new List<int>();

                            foreach (var userId in currentUserIds.Except(assignedUserIds))
                            {
                                await _httpClient.DeleteAsync($"{_apiUrl}/api/DefaultShiftPattern/{id}/User/{userId}");
                            }

                            foreach (var userId in assignedUserIds.Except(currentUserIds))
                            {
                                await _httpClient.PostAsync($"{_apiUrl}/api/DefaultShiftPattern/{id}/User/{userId}", null);
                            }
                        }

                        return RedirectToAction(nameof(Index));
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = errorMessage;
                    }
                    else
                    {
                        TempData["Error"] = $"Failed to update shift pattern. " + response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(await ReloadViewModelWithUsers(pattern));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/DefaultShiftPattern/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Could not delete the shift pattern.";
            return RedirectToAction(nameof(Index));
        }

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

        private async Task<DefaultShiftPatternCreateViewModel> ReloadViewModelWithUsers(DefaultShiftPattern pattern, int pageNumber = 1, int pageSize = 20)
        {
            var usersApiUrl = ApiRoutes.User.GetPaginated(pageNumber, pageSize);
            var userResponse = await _httpClient.GetAsync(usersApiUrl);
            
            var viewModel = new DefaultShiftPatternCreateViewModel
            {
                Pattern = pattern,
                AvailableUsers = new PaginatedResult<User>(),
                AssignedUsers = pattern.Users?.ToList() ?? new List<User>()
            };

            if (userResponse.IsSuccessStatusCode)
            {
                var usersJson = await userResponse.Content.ReadAsStringAsync();
                viewModel.AvailableUsers = JsonSerializer.Deserialize<PaginatedResult<User>>(
                    usersJson, JsonOptions.Default);
            }

            return viewModel;
        }
    }
}