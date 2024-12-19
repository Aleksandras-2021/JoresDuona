using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Text.Json;
using System.Text;
using PosShared;
using PosShared.ViewModels;
using PosClient.Services;
using PosShared.DTOs;

namespace PosClient.Controllers;

public class UserController : Controller
{
    private readonly ApiService _apiService;

    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

    public UserController(ApiService apiService)
    {
        _apiService = apiService;

    }

    // GET: User/Index
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var apiUrl = ApiRoutes.User.ListPaginated(pageNumber, pageSize);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<PaginatedResult<User>>(jsonData, JsonOptions.Default);
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
        if (ModelState.IsValid)
        {
            var apiUrl = ApiRoutes.User.Create;
            var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");

            var response = await _apiService.PostAsync(apiUrl, content);

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
        var apiUrl = ApiRoutes.User.GetById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if(response.IsSuccessStatusCode)
        {
            string userData = await response.Content.ReadAsStringAsync();
            UserDTO? user = JsonSerializer.Deserialize<UserDTO>(userData,JsonOptions.Default);
            
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

            var response = await _apiService.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = $"Failed to update user. Status code:{response.StatusCode}";
        }

        return View(user);
    }

    // GET: User/Delete/
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var apiUrl = ApiRoutes.User.GetById(id);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var userData = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(userData,JsonOptions.Default);

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
        var apiUrl = ApiRoutes.User.Delete(id);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = $"Deleted user {id}. Status code:{response.StatusCode}";
            return RedirectToAction("Index");
        }

        TempData["Error"] = $"Failed to delete user. Status code:{response.StatusCode}";
        return RedirectToAction("Index");
    }
    
}
