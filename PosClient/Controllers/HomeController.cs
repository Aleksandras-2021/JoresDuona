using System.Text;
using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using PosShared;
using PosClient.Services;

namespace PosClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IUserSessionService _userSessionService;
        private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

        public HomeController(ApiService apiService, IUserSessionService userSessionService)
        {
            _apiService = apiService;
            _userSessionService = userSessionService;
        }

        // GET: Home/Index
        public ActionResult Index()
        {
            // Check if the user is authenticated by checking if the token cookie is present
            if (Request.Cookies["authToken"] == null)
            {
                // If not authenticated, redirect to login page
                return RedirectToAction("Login");
            }
            var role = _userSessionService.GetCurrentUserRole();
            ViewData["UserRole"] = role.ToString();

            return View();
        }



        // GET: Home/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var loginRequest = new { Email = email, Password = password };
            
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await _apiService.PostAsync(ApiRoutes.Auth.Login, content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseData);

                string token = jsonDocument.RootElement.GetProperty("token").GetString();
                int userId = jsonDocument.RootElement.GetProperty("id").GetInt32();
                string userEmail = jsonDocument.RootElement.GetProperty("email").GetString();
                int roleValue = jsonDocument.RootElement.GetProperty("role").GetInt32();

                UserRole role = (UserRole)roleValue;

                _userSessionService.SetCurrentUserId(userId);
                _userSessionService.SetCurrentUserEmail(userEmail);
                _userSessionService.SetCurrentUserRole(role);

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("UserEmail", userEmail);
                HttpContext.Session.SetString("UserRole", role.ToString());

                // Store the JWT token in a cookie (HttpOnly, Secure)
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SameSite = SameSiteMode.None  // Important for cross-origin requests,
                };

                // Set the cookie with the token
                Response.Cookies.Append("authToken", token, cookieOptions);

                // Redirect to home page after successful login
                return RedirectToAction("Index");
            }

            // If login failed, display an error
            TempData["Error"] = "Invalid credentials.";
            return RedirectToAction("Login");
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            // Prepare the request payload
            var changePasswordRequest = new
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            var content = new StringContent(JsonSerializer.Serialize(changePasswordRequest), Encoding.UTF8, "application/json");
            
            var response = await _apiService.PostAsync(ApiRoutes.Auth.ChangePassword, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to change password: {errorMessage}";
                return RedirectToAction("ChangePassword");
            }
        }

        // GET: Home/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        
        public IActionResult Logout()
        {

            // Delete Cookie
            Response.Cookies.Delete("authToken");
            Response.Cookies.Delete("cachedItems");

            HttpContext.Session.Clear();

            // Redirect to home page after successful login
            return RedirectToAction("Login");
        }
    }
}
