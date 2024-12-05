﻿using Microsoft.AspNetCore.Mvc;
using PosShared.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using PosClient.Services;
using PosShared;
using PosShared.ViewModels;
using System.Text;
using PosShared.DTOs;

namespace PosClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IUserSessionService _userSessionService;


        private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

        public OrderController(HttpClient httpClient, IUserSessionService userSessionService)
        {
            _httpClient = httpClient;
            _userSessionService = userSessionService;
        }
        //Get api/Order
        public async Task<IActionResult> Index()
        {

            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + "/api/Order";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(orders);
            }

            // Handle errors or empty results
            TempData["Error"] = "Could not retrieve users.";
            return View(new List<Order>());
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + "/api/Order";
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Optional: Log or parse the created order if needed
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var createdOrder = JsonSerializer.Deserialize<Order>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Redirect to Index after successful creation
                return RedirectToAction("SelectItems", new { orderId = createdOrder?.Id });
            }

            // Handle errors
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error creating order: {errorMessage}");

            // Redirect to Index even if there was an error, you could also render an error-specific view if desired
            TempData["Error"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> SelectItems()
        {
            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + "/api/Order";
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Optional: Log or parse the created order if needed
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var createdOrder = JsonSerializer.Deserialize<Order>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Redirect to Index after successful creation
                return RedirectToAction(nameof(Index));
            }

            // Handle errors
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error creating order: {errorMessage}");

            // Redirect to Index even if there was an error, you could also render an error-specific view if desired
            TempData["Error"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SelectItems(int orderId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Check if items are available in the cookie
            string? itemsCookie = Request.Cookies["cachedItems"];
            List<Item>? items;

            if (!string.IsNullOrEmpty(itemsCookie))
            {
                // Deserialize items from the cookie
                items = JsonSerializer.Deserialize<List<Item>>(itemsCookie, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                // Fetch available items from the API
                var itemsApiUrl = _apiUrl + "/api/Items";
                var itemsResponse = await _httpClient.GetAsync(itemsApiUrl);

                if (itemsResponse.IsSuccessStatusCode)
                {
                    var itemsJson = await itemsResponse.Content.ReadAsStringAsync();
                    items = JsonSerializer.Deserialize<List<Item>>(itemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Store items in a cookie for 2 minutes
                    if (items != null)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.UtcNow.AddMinutes(1),
                            HttpOnly = true,
                            Secure = true
                        };
                        Response.Cookies.Append("cachedItems", JsonSerializer.Serialize(items), cookieOptions);
                    }
                }
                else
                {
                    items = new List<Item>();
                }
            }

            // Fetch existing order items
            var orderItemsApiUrl = $"{_apiUrl}/api/Order/{orderId}/OrderItems";
            var orderItemsResponse = await _httpClient.GetAsync(orderItemsApiUrl);

            List<OrderItem>? orderItems = null;
            if (orderItemsResponse.IsSuccessStatusCode)
            {
                var orderItemsJson = await orderItemsResponse.Content.ReadAsStringAsync();
                orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var model = new SelectItemsViewModel
            {
                OrderId = orderId,
                Items = items,
                OrderItems = orderItems
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Order/{orderId}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the index after successfully deleting the order
                TempData["Message"] = "Order deleted successfully.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Could not delete the order.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> AddItemToOrder(int orderId, int itemId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Order/{orderId}/AddItem";
            var content = new StringContent(JsonSerializer.Serialize(new { ItemId = itemId }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the SelectItems page to add more items
                return RedirectToAction("SelectItems", new { orderId });
            }

            TempData["Error"] = "Could not add item to order.";
            return RedirectToAction("SelectItems", new { orderId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItemFromOrder(int orderId, int orderItemId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_apiUrl}/api/Order/{orderId}/DeleteItem/{orderItemId}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the SelectItems page to view the updated list
                return RedirectToAction("SelectItems", new { orderId });
            }

            TempData["Error"] = "Could not remove item from order.";
            return RedirectToAction("SelectItems", new { orderId });
        }




    }
}