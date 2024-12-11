using Microsoft.AspNetCore.Mvc;
using PosClient.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using PosShared.ViewModels;

namespace PosClient.Controllers;

public class PaymentController : Controller
{

    private readonly HttpClient _httpClient;
    private readonly IUserSessionService _userSessionService;


    private readonly string _apiUrl = UrlConstants.ApiBaseUrl;

    public PaymentController(HttpClient httpClient, IUserSessionService userSessionService)
    {
        _httpClient = httpClient;
        _userSessionService = userSessionService;
    }

    [HttpGet]
    public IActionResult Create(int orderId, decimal untaxedAmount, decimal tax)
    {
        // Initialize the PaymentViewModel using the query string parameters
        var paymentViewModel = new PaymentViewModel
        {
            OrderId = orderId,
            UntaxedAmount = untaxedAmount,
            TaxAmount = tax,
            TotalAmount = untaxedAmount + tax
        };

        return View(paymentViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaymentViewModel paymentViewModel)
    {
        string? token = Request.Cookies["authToken"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        AddPaymentDTO payment = new AddPaymentDTO()
        {
            Amount = paymentViewModel.Amount,
            OrderId = paymentViewModel.OrderId,
            PaymentMethod = paymentViewModel.PaymentMethod
        };

        var content = new StringContent(JsonSerializer.Serialize(payment), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_apiUrl + $"/api/Payment", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", "Order");
        }

        TempData["Error"] = "Failed to create Payment. Please try again.\n" + response.ToString();
        return View(paymentViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderPayments(int orderId)
    {
        string? token = Request.Cookies["authToken"];

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiUrl = _apiUrl + $"/api/Payment/Order/{orderId}";
        var response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var paymentData = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment>>(paymentData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payments != null && payments.Any())
            {
                var model = new OrderPaymentViewModel()
                {
                    OrderId = orderId,
                    Payments = payments
                };

                return View("OrderPayments", model);
            }
        }

        TempData["Error"] = "Unable to fetch order payments. Most likely they do not exist.";
        return RedirectToAction("Index", "Order");
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderReceipt(int orderId)
    {
        string? token = Request.Cookies["authToken"];

        if (string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Authentication token is missing.";
            return Unauthorized();
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            // Get Order Items
            var orderItemsApiUrl = _apiUrl + $"/api/Order/{orderId}/Items";
            var orderItemsResponse = await _httpClient.GetAsync(orderItemsApiUrl);

            if (!orderItemsResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to fetch order items.";
                return View("Receipt", new ReceiptViewModel() { OrderId = orderId });
            }

            var orderItemsData = await orderItemsResponse.Content.ReadAsStringAsync();
            List<OrderItem>? orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (orderItems == null || !orderItems.Any())
            {
                TempData["Error"] = "No order items found.";
                orderItems = new List<OrderItem>();
            }

            // Get Order Item Variations
            var orderItemVariationsApiUrl = _apiUrl + $"/api/Order/{orderId}/Variations";
            var orderItemVariationsResponse = await _httpClient.GetAsync(orderItemVariationsApiUrl);

            if (!orderItemVariationsResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to fetch order item variations.";
                return View("Receipt", new ReceiptViewModel() { OrderId = orderId, OrderItems = orderItems }); // Return with items but no variations
            }

            var orderItemVariationsData = await orderItemVariationsResponse.Content.ReadAsStringAsync();
            List<OrderItemVariation>? orderItemVariations = JsonSerializer.Deserialize<List<OrderItemVariation>>(orderItemVariationsData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (orderItemVariations == null || !orderItemVariations.Any())
            {
                TempData["Error"] = "No variations found for order items.";
                orderItemVariations = new List<OrderItemVariation>();
            }

            decimal totalTax = 0;
            decimal totalCharge = 0;
            decimal total = 0;


            foreach (var item in orderItems) 
            {
                totalCharge += item.Price * item.Quantity;
                totalTax += item.TaxedAmount * item.Quantity;
            }

            foreach (var variation in orderItemVariations)
            {
                totalCharge += variation.AdditionalPrice * variation.Quantity;
                totalTax += variation.TaxedAmount * variation.Quantity;
            }
            total = totalTax + totalCharge;


            var model = new ReceiptViewModel()
            {
                OrderId = orderId,
                OrderItems = orderItems,
                OrderItemVariatons = orderItemVariations,
                Total = total,
                TotalCharge = totalCharge,
                TotalTax = totalTax,
            };

            return View("Receipt", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An unexpected error occurred: {ex.Message}";
            return View("Receipt", new ReceiptViewModel() { OrderId = orderId });
        }
    }
}
