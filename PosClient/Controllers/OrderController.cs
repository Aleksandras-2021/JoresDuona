using Microsoft.AspNetCore.Mvc;
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

            // Fetch available items from the API
            var itemsApiUrl = $"{_apiUrl}/api/Items";
            var itemsResponse = await _httpClient.GetAsync(itemsApiUrl);

            List<Item>? items = null;
            if (itemsResponse.IsSuccessStatusCode)
            {
                var itemsJson = await itemsResponse.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Item>>(itemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                items = new List<Item>();
            }

            // Fetch existing order items from the API
            var orderItemsApiUrl = $"{_apiUrl}/api/Order/{orderId}/OrderItems";
            var orderItemsResponse = await _httpClient.GetAsync(orderItemsApiUrl);

            List<OrderItem>? orderItems = null;
            if (orderItemsResponse.IsSuccessStatusCode)
            {
                var orderItemsJson = await orderItemsResponse.Content.ReadAsStringAsync();
                orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                orderItems = new List<OrderItem>();
            }

            // Prepare the view model
            var model = new SelectItemsViewModel
            {
                OrderId = orderId,
                Items = items.Where(Item => Item.Quantity > 0).ToList(),
                OrderItems = orderItems
            };

            //Fetch variations

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

        [HttpPost]
        public async Task<IActionResult> AddItemVariationToOrderItem(int varId, int orderItemId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // API endpoint to add variation to order item
            var apiUrl = $"{_apiUrl}/api/OrderItems/{orderItemId}/AddVariation";

            // Request body containing the variation ID
            var content = new StringContent(JsonSerializer.Serialize(new { VariationId = varId }), Encoding.UTF8, "application/json");

            // Call the API
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the SelectItems page to view updated order items
                TempData["Message"] = "Variation added successfully.";
                return RedirectToAction("SelectItems", new { orderId = GetOrderIdFromOrderItem(orderItemId) });
            }

            // Handle errors
            TempData["Error"] = "Could not add variation to order item.";
            return RedirectToAction("SelectItems", new { orderId = GetOrderIdFromOrderItem(orderItemId) });
        }




        // Helper method to fetch the order ID from the order item ID
        private async Task<int> GetOrderIdFromOrderItem(int orderItemId)
        {
            var apiUrl = _apiUrl + $"/api/Order/OrderItems/{orderItemId}";
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Optional: Log or parse the created order if needed
                var jsonResponse = await response.Content.ReadAsStringAsync();
                OrderItem orderItem = JsonSerializer.Deserialize<OrderItem>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return orderItem.OrderId;
            }

            return 0;

        }
    }
}
