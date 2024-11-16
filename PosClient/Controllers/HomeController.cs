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

namespace PosClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HttpClient httpClient, ILogger<HomeController> logger)
        {
            _httpClient = httpClient;
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
                // Extract token from the response JSON (assuming token is returned in JSON format)
                var responseData = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseData);
                string token = jsonDocument.RootElement.GetProperty("token").GetString(); // Modify if your API structure is different

                // Store the JWT token in a cookie (HttpOnly, Secure)
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Use true in production
                    Expires = DateTime.UtcNow.AddMinutes(10), // Adjust based on your needs
                    SameSite = SameSiteMode.None  // Important for cross-origin requests, if applicable
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
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string email, string password)
        {

            // Delete Cookie
            Response.Cookies.Delete("authToken");
            HttpContext.Session.Clear();

            // Redirect to home page after successful login
            return RedirectToAction("Login");
        }




        [HttpPost]
        public async Task<IActionResult> GetBusinessById(int businessId)
        {
            string? token = Request.Cookies["authToken"]; // Retrieve the token from cookies

            int? userId = Ultilities.ExtractUserIdFromToken(token); //Extract userId from Token

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Put the token into authroization header

            var apiUrl = _apiUrl + $"/api/Businesses/{businessId}";

            var response = await _httpClient.GetAsync(apiUrl); //

            if (response.IsSuccessStatusCode)
            {
                var businessJson = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var business = JsonSerializer.Deserialize<Business>(businessJson, options);

                TempData["BusinessData"] = JsonSerializer.Serialize(business);
                return RedirectToAction("Result");
            }

            TempData["BusinessData"] = "Business not found or an error occurred.";
            return RedirectToAction("Result");
        }


        // GET: Home/Result
        public IActionResult Result()
        {
            var jsonData = TempData["BusinessData"]?.ToString();
            Business? business = null;

            if (!string.IsNullOrEmpty(jsonData))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                business = JsonSerializer.Deserialize<Business>(jsonData, options);
            }

            return View(business); // Pass the object to the view
        }

    }
}
