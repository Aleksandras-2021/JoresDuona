using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.ViewModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PosClient.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("http://localhost:5149/api/Users");
            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new User object based on the ViewModel
                var user = new User
                {
                    BusinessId = model.BusinessId,
                    Username = model.Username,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    Role = model.Role,
                    EmploymentStatus = model.EmploymentStatus
                };

                // Hash the password entered by the user
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }
                else
                {
                    // Password is required, you can add custom validation here
                    ViewBag.ErrorMessage = "Password is required.";
                    return View(model);
                }

                // Send the User object to your API
                var apiUrl = "http://localhost:5149/api/Users";
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle error response
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to create user. Error: {response.StatusCode} - {errorResponse}";
                }
            }

            ViewBag.ErrorMessage = "Model is not valid.";
            return View(model);
        }

        // GET: Business/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Users/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(userData);

                if (user != null)
                {
                    return View(user);
                }
            }

            return NotFound(); // Return a 404 if the business was not found or request failed
        }

        // POST: User/Edit/
        [HttpPost]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                var apiUrl = $"http://localhost:5149/api/Users/{id}";
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to the Index action after successful edit
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to update business.";
            }

            return View(user); // Return to the edit view if validation fails or update fails
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Users/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(userData);

                if (user != null)
                {
                    return View(user);
                }
            }

            return NotFound(); // Return a 404 if the business was not found or request failed
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiUrl = $"http://localhost:5149/api/Users/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the Index action after successful deletion
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to delete business.";
            return RedirectToAction("Index");
        }

    }
}
