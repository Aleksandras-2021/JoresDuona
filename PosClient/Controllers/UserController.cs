using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using PosShared;

namespace PosClient.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: User/Index
        public async Task<IActionResult> Index()
        {
            var apiUrl = _apiUrl + "/api/Users";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(users);
            }

            // Handle errors or empty results
            ViewBag.ErrorMessage = "Could not retrieve users.";
            return View(new List<User>());
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var apiUrl = _apiUrl + "/api/Users";
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                // Handle error response
                ViewBag.ErrorMessage = "Failed to create user.";
            }

            return View(user);
        }

        // GET: User/Edit/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var apiUrl = _apiUrl + $"/api/Users/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string userData = await response.Content.ReadAsStringAsync();
                User user = JsonSerializer.Deserialize<User>(userData);
                Console.WriteLine(userData);
                Console.WriteLine(user.Id);
                


                if (user != null)
                {
                    return View(user);
                }
            }

            return NotFound();
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
                Console.WriteLine("Im in Edit user");
                var apiUrl = _apiUrl + $"/api/Users/{id}";
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                Console.WriteLine("User Edit: " + response);


                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Failed to update user.";
            }

            return View(user); // Return to the edit view if validation fails or update fails
        }

        // GET: User/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var apiUrl = _apiUrl + $"/api/Users/{id}";
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

            return NotFound(); // Return a 404 if the user was not found or request failed
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiUrl = _apiUrl + $"/api/Users/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to delete user.";
            return RedirectToAction("Index");
        }
    }
}
