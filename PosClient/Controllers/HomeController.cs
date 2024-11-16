using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PosShared;

namespace PosClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        // POST: Home/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var loginRequest = new { Email = email, Password = password };

            // Call API to validate login
            var response = await _httpClient.PostAsJsonAsync(_apiUrl + "/api/login", loginRequest);
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                // Store the JWT token in a cookie (HttpOnly, Secure)
                var token = await response.Content.ReadAsStringAsync();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(10), // Session cookie if coomented
                    SameSite = SameSiteMode.None  // Important for cross-origin requests
                };

                Response.Cookies.Append("authToken", token, cookieOptions);

                // Check if the cookie is successfully set and redirect accordingly
                if (Request.Cookies["authToken"] != null)
                {
                    // Redirect to the home page after successful login
                    return RedirectToAction("Index");
                }
            }

            // If login failed, display an error
            TempData["Error"] = "Invalid credentials.";
            return RedirectToAction("Login");
        }


        // POST: Home/GetBusinessById
        [HttpPost]
        public async Task<IActionResult> GetBusinessById(int businessId)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var apiUrl = _apiUrl + $"/api/Businesses/{businessId}";
            var response = await _httpClient.GetAsync(apiUrl);

            Console.WriteLine(response.ToString());

            if (response.IsSuccessStatusCode)
            {
                var businessJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content: " + businessJson);

                // Deserialize the JSON response into a Business object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var business = JsonSerializer.Deserialize<Business>(businessJson, options);

                // Store the deserialized business object in TempData (serialize it back to string)
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
