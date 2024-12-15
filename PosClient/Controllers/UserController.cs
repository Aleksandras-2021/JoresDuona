using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using PosShared;
using PosShared.ViewModels;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Net.Http.Headers;
using PosShared.DTOs;

namespace PosClient.Controllers;

public class UserController : Controller
{
    private readonly HttpClient _httpClient;

    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

    public UserController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET: User/Index
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = ApiRoutes.User.GetPaginated(pageNumber, pageSize);
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<PaginatedResult<User>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(users);
        }

        TempData["Error"] = "Could not retrieve users." + response.Content;
        return View(new PaginatedResult<User>());
    }

    // GET: User/Create
    public IActionResult Create()
    {
        return View(new CreateUserDTO());
    }

    // POST: User/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDTO newUser)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var apiUrl = ApiRoutes.User.Create;
            var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to create user.";
        }

        return View(newUser);
    }


    // GET: User/Edit/
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = ApiRoutes.User.GetById(id);
        var response = await _httpClient.GetAsync(apiUrl);

        if(response.IsSuccessStatusCode)
        {
            string userData = await response.Content.ReadAsStringAsync();
            UserDTO? user = JsonSerializer.Deserialize<UserDTO>(userData,new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            Console.WriteLine(response);
            
            if (user != null)
            {
                UserViewModel model = new UserViewModel()
                {
                    Id = user.Id,
                    BusinessId = user.BusinessId,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    EmploymentStatus = user.EmploymentStatus
                };

                return View(model);
            }
        }

        return NotFound();
    }

    // POST: User/Edit/
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserViewModel user)
    {

        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var apiUrl = ApiRoutes.User.Update(id);
            
            UserDTO dto = new UserDTO();
            dto.Id = user.Id;
            dto.BusinessId = user.BusinessId;
            dto.Username = user.Username;
            dto.Name = user.Name;
            dto.Email = user.Email;
            dto.Phone = user.Email;
            dto.Address = user.Address;
            dto.Role = user.Role;
            dto.EmploymentStatus = user.EmploymentStatus;
            
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update user.";
        }

        return View(user);
    }

    // GET: User/Delete/
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = ApiRoutes.User.GetById(id);
        var response = await _httpClient.GetAsync(apiUrl);
        Console.WriteLine(response);

        if (response.IsSuccessStatusCode)
        {
            var userData = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(userData);

            if (user != null)
            {
                return View(user);
            }
        }

        return NotFound();
    }

    // POST: User/Delete/
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = ApiRoutes.User.Delete(id);
        var response = await _httpClient.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Failed to delete user.";
        return RedirectToAction("Index");
    }
    
}
