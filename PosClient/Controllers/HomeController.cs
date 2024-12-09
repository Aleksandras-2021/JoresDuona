using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PosShared;
using System.IdentityModel.Tokens.Jwt;
using PosShared.Ultilities;
using PosClient.Services;
using System.Net;

namespace PosClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IUserSessionService _userSessionService;
        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HttpClient httpClient, IUserSessionService userSessionService, ILogger<HomeController> logger)
        {
            _httpClient = httpClient;
            _userSessionService = userSessionService;
            _logger = logger;
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

            // Call API to validate login
            var response = await _httpClient.PostAsJsonAsync(_apiUrl + "/api/login", loginRequest);

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
