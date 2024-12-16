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

namespace PosClient.Controllers;

public class OrderController : Controller
{
    private readonly ApiService _apiService;
    
    public OrderController(ApiService apiService)
    {
        _apiService = apiService;
    }
    //Get api/Order
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
    {
        var apiUrl = ApiRoutes.Order.GetPaginated(pageNumber, pageSize);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaginatedResult<Order>>(jsonData, JsonOptions.Default);

            return View(result);
        }

        TempData["Error"] = "No orders to retrieve.";
        return View(new PaginatedResult<Order>());
    }

    // POST: Order/Create
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var apiUrl = ApiRoutes.Order.Create;
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<Order>(jsonResponse, JsonOptions.Default);

            // Redirect to Index after successful creation
            return RedirectToAction("SelectItems", new { orderId = createdOrder?.Id });
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating order: {errorMessage}");

        TempData["Error"] = errorMessage;
        return RedirectToAction(nameof(Index));
    }

    // POST: Order/Create
    [HttpPost]
    public async Task<IActionResult> SelectItems()
    {
        var apiUrl = ApiRoutes.Order.Create;

        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<Order>(jsonResponse, JsonOptions.Default);

            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, $"Error creating order: {errorMessage}");

        TempData["Error"] = errorMessage;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> SelectItems(int orderId,int pageNumber = 1, int pageSize = 20)
    {
        var itemsApiUrl = ApiRoutes.Items.GetItemsPaginated(pageNumber,pageSize);
        var itemsResponse = await _apiService.GetAsync(itemsApiUrl);

        PaginatedResult<Item>? items = null;
        if (itemsResponse.IsSuccessStatusCode)
        {
            var itemsJson = await itemsResponse.Content.ReadAsStringAsync();
            items = JsonSerializer.Deserialize<PaginatedResult<Item>>(itemsJson, JsonOptions.Default);
        }
        else
        {
            items = new PaginatedResult<Item>();
        }

        var orderItemsApiUrl = ApiRoutes.OrderItems.GetOrderItems(orderId);
        var orderItemsResponse = await _apiService.GetAsync(orderItemsApiUrl);

        List<OrderItem>? orderItems = null;
        if (orderItemsResponse.IsSuccessStatusCode)
        {
            var orderItemsJson = await orderItemsResponse.Content.ReadAsStringAsync();
            orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsJson, JsonOptions.Default);
        }
        else
        {
            orderItems = new List<OrderItem>();
        }
        
        var orderServicesApiUrl = ApiRoutes.Order.GetOrderServices(orderId);
        var orderServicesResponse = await _apiService.GetAsync(orderServicesApiUrl);

        List<PosShared.Models.OrderService>? orderServices = null;
        if (orderServicesResponse.IsSuccessStatusCode)
        {
            var orderServicesJson = await orderServicesResponse.Content.ReadAsStringAsync();
            orderServices = JsonSerializer.Deserialize<List<PosShared.Models.OrderService>>(orderServicesJson, JsonOptions.Default);
        }
        else
        {
            orderServices = new List<PosShared.Models.OrderService>();
        }
        

        var orderApiUrl = ApiRoutes.Order.GetById(orderId);
        var orderResponse = await _apiService.GetAsync(orderApiUrl);
        Order? order = null;
        if (orderResponse.IsSuccessStatusCode)
        {
            var orderJson = await orderResponse.Content.ReadAsStringAsync();
            order = JsonSerializer.Deserialize<Order>(orderJson, JsonOptions.Default);
        }
        else
        {
            TempData["Error"] = "Cannot fetch the order";
        }

        var model = new SelectItemsViewModel
        {
            OrderId = orderId,
            Order = order,
            Items = items,
            OrderItems = orderItems,
            OrderServices = orderServices
        };
        
        return View(model);
    }
    
    // GET: Order/Edit/
    [HttpGet]
    public async Task<IActionResult> Edit(int orderId)
    {
        var apiUrl = ApiRoutes.Order.GetById(orderId);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var orderData = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<Order>(orderData, JsonOptions.Default);

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
        
        if (ModelState.IsValid)
        {
            var apiUrl = ApiRoutes.Order.Update(order.Id);
            var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");

            var response = await _apiService.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update item.";
        }

        return View(order);
    }


    [HttpPost]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        var apiUrl = ApiRoutes.Order.Delete(orderId);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Order deleted successfully.";
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Could not delete the order.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> AddItemToOrder(int orderId, int itemId)
    {
        var apiUrl = ApiRoutes.OrderItems.AddOrderItem(orderId);
        var content = new StringContent(JsonSerializer.Serialize(new { ItemId = itemId }), Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("SelectItems", new { orderId });
        }

        TempData["Error"] = "Could not add item to order.";
        return RedirectToAction("SelectItems", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItemFromOrder(int orderId, int orderItemId)
    {
        var apiUrl = ApiRoutes.OrderItems.DeleteOrderItem(orderId, orderItemId);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("SelectItems", new { orderId });
        }

        TempData["Error"] = "Could not remove item from order.";
        return RedirectToAction("SelectItems", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> AddItemVariationToOrderItem(int varId, int orderItemId, int orderId, int itemId)
    {
        var apiUrl = ApiRoutes.OrderItems.AddVariation(orderId, orderItemId);

        var addVariationDTO = new
        {
            VariationId = varId,
            Quantity = 1
        };

        var content = new StringContent(JsonSerializer.Serialize(addVariationDTO), Encoding.UTF8, "application/json");

        var response = await _apiService.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Variation added successfully.";
            return RedirectToAction("ItemVariations", new { itemId, orderItemId, orderId });
        }

        // Handle errors
        TempData["Error"] = "Could not add variation to order item.";
        return RedirectToAction("ItemVariations", new { itemId, orderItemId, orderId });
    }

    // GET: Items/{id}/Variations
    // GET: Order/{orderId}/Items/{orderItemId}/Variations
    public async Task<IActionResult> ItemVariations(int itemId, int orderItemId, int orderId)
    {
        var variationsApiUrl = ApiRoutes.Items.GetItemVariations(itemId);
        var response = await _apiService.GetAsync(variationsApiUrl);

        List<ItemVariation>? variations = null;
        if (response.IsSuccessStatusCode)
        {
            var variationsJson = await response.Content.ReadAsStringAsync();
            variations = JsonSerializer.Deserialize<List<ItemVariation>>(variationsJson, JsonOptions.Default);
        }
        else
        {
            variations = new List<ItemVariation>();
        }

        var orderItemVariationsApiUrl = ApiRoutes.OrderItems.GetVariations(orderId, orderItemId);
        response = await _apiService.GetAsync(orderItemVariationsApiUrl);

        List<OrderItemVariation>? orderItemVariations = null;
        if (response.IsSuccessStatusCode)
        {
            var variationsJson = await response.Content.ReadAsStringAsync();
            orderItemVariations = JsonSerializer.Deserialize<List<OrderItemVariation>>(variationsJson, JsonOptions.Default);
        }
        else
        {
            orderItemVariations = new List<OrderItemVariation>();
        }

        // Prepare the model for the view
        var model = new ItemVariationsViewModel
        {
            ItemId = itemId,
            OrderItemId = orderItemId,
            OrderId = orderId,
            Variations = variations,
            OrderItemVariations = orderItemVariations
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteVariation(int orderId, int orderItemVariationId, int orderItemId, int itemId)
    {
        var apiUrl = ApiRoutes.OrderItems.DeleteVariation(orderId, orderItemId, orderItemVariationId);
        var response = await _apiService.DeleteAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Variation deleted successfully.";
            return RedirectToAction("ItemVariations", new { itemId, orderItemId, orderId });
        }

        TempData["Error"] = "Could not delete the Variation.";
        return RedirectToAction("ItemVariations", new { itemId, orderItemId, orderId });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        var apiUrl = ApiRoutes.Order.UpdateStatus(orderId, status);
        var response = await _apiService.PostAsync(apiUrl, null);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Order closed successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to close the order.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RedirectToPayment(int orderId, decimal untaxedAmount, decimal tax)
    {
        return RedirectToAction("Create", "Payment", new { orderId, untaxedAmount, tax });
    }

    [HttpGet]
    public IActionResult RedirectToGetPayment(int orderId)
    {
        return RedirectToAction("GetOrderPayments", "Payment", new { orderId });
    }

    [HttpGet]
    public IActionResult RedirectToReceipt(int orderId)
    {
        return RedirectToAction("GetOrderReceipt", "Payment", new { orderId });
    }

}
