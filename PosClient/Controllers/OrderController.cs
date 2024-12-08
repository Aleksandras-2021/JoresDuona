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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

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

            // Fetch existing order
            var orderApiUrl = $"{_apiUrl}/api/Order/{orderId}";
            var orderResponse = await _httpClient.GetAsync(orderApiUrl);
            Order? order = null;
            if (orderResponse.IsSuccessStatusCode)
            {
                var orderJson = await orderResponse.Content.ReadAsStringAsync();
                order = JsonSerializer.Deserialize<Order>(orderJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                TempData["Error"] = "Cannot fetch the order";
            }

            // Prepare the view model
            var model = new SelectItemsViewModel
            {
                OrderId = orderId,
                Order = order,
                Items = items.Where(Item => Item.Quantity > 0).ToList(),
                OrderItems = orderItems
            };


            return View(model);
        }



        // GET: Order/Edit/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _apiUrl + $"/api/Order/{id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var orderData = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<Order>(orderData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (order != null)
                {
                    return View(order);
                }
            }

            TempData["Error"] = "Unable to fetch order details. Please try again.";
            return RedirectToAction("Index");
        }


        // POST: Order/Edit/
        [HttpPost]
        public async Task<IActionResult> Edit(Order order)
        {

            string? token = Request.Cookies["authToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {
                var apiUrl = _apiUrl + $"/api/Order/{order.Id}";
                var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Failed to update item.";
            }

            return View(order); // Return to the edit view if validation fails or update fails
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
        public async Task<IActionResult> AddItemVariationToOrderItem(int varId, int orderItemId, int orderId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // API endpoint to add variation to order item
            var apiUrl = $"{_apiUrl}/api/Order/{orderId}/OrderItems/{orderItemId}/AddVariation";

            // Construct the DTO object
            var addVariationDTO = new
            {
                VariationId = varId,
                Quantity = 1
            };

            // Serialize the DTO to JSON
            var content = new StringContent(JsonSerializer.Serialize(addVariationDTO), Encoding.UTF8, "application/json");

            // Call the API
            var response = await _httpClient.PostAsync(apiUrl, content);
            Console.WriteLine(response.ToString());

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the SelectItems page to view updated order items
                TempData["Message"] = "Variation added successfully.";
                return RedirectToAction("SelectItems", new { orderId = orderId });
            }

            // Handle errors
            TempData["Error"] = "Could not add variation to order item.";
            return RedirectToAction("SelectItems", new { orderId = orderId });
        }


        // GET: Order/ItemVariations
        public async Task<IActionResult> ItemVariations(int itemId, int orderItemId, int orderId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Fetch item variations from the API
            var variationsApiUrl = $"{_apiUrl}/api/Items/{itemId}/Variations";
            var response = await _httpClient.GetAsync(variationsApiUrl);

            List<ItemVariation>? variations = null;
            if (response.IsSuccessStatusCode)
            {
                var variationsJson = await response.Content.ReadAsStringAsync();
                variations = JsonSerializer.Deserialize<List<ItemVariation>>(variationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                variations = new List<ItemVariation>();
            }

            // Prepare the model for the view
            var model = new ItemVariationsViewModel
            {
                ItemId = itemId,
                OrderItemId = orderItemId,
                OrderId = orderId,
                Variations = variations
            };

            return View(model);
        }


        public async Task<IActionResult> GetOrderItemVariations(int orderItemId, int orderId)
        {
            string? token = Request.Cookies["authToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Fetch item variations from the API
            var variationsApiUrl = $"{_apiUrl}/api/Order/{orderId}/OrderItems/{orderItemId}/OrderItemVariations";
            var response = await _httpClient.GetAsync(variationsApiUrl);

            List<ItemVariation>? variations = null;

            if (response.IsSuccessStatusCode)
            {
                var variationsJson = await response.Content.ReadAsStringAsync();
                variations = JsonSerializer.Deserialize<List<ItemVariation>>(variationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Ensure variations is initialized
            variations ??= new List<ItemVariation>();

            if (!variations.Any())
            {
                TempData["Error"] = "No variations found";
                return RedirectToAction("SelectItems", new { orderId = orderId });
            }

            var model = new ItemVariationsViewModel
            {
                ItemId = variations.First().ItemId,
                OrderItemId = orderItemId,
                OrderId = orderId,
                Variations = variations
            };

            return View("SelectedVariations", model);
        }



        public IActionResult RedirectToPayment(Order order)
        {
            return RedirectToAction("Create", "Payment", new { order });
        }




    }
}
