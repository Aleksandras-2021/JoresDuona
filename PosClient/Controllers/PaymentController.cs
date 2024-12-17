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


    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;

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
            var payments = JsonSerializer.Deserialize<List<Payment>>(paymentData,JsonOptions.Default);

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
            var orderApiUrl = ApiRoutes.Order.GetById(orderId);
            var orderResponse = await _httpClient.GetAsync(orderApiUrl);
            Order? order = null;

            if (orderResponse.IsSuccessStatusCode)
            {
                var orderData = await orderResponse.Content.ReadAsStringAsync();
                order = JsonSerializer.Deserialize<Order>(orderData, JsonOptions.Default);
            }
            else
            {
                TempData["Error"] = "Couldn't retrieve order.";

            }
            
            
            
            // Get Order Items
            var orderItemsApiUrl = ApiRoutes.OrderItems.GetOrderItems(orderId);
            var orderItemsResponse = await _httpClient.GetAsync(orderItemsApiUrl);

            List<OrderItem>? orderItems = null;

            if (orderItemsResponse.IsSuccessStatusCode)
            {
                var orderItemsData = await orderItemsResponse.Content.ReadAsStringAsync();
                 orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsData, JsonOptions.Default);
            }
            else
            {
                orderItems = new List<OrderItem>();
            }

            // Get Order Item Variations
            var orderItemVariationsApiUrl = ApiRoutes.Order.GetOrderVariations(orderId);
            var orderItemVariationsResponse = await _httpClient.GetAsync(orderItemVariationsApiUrl);

            List<OrderItemVariation>? orderItemVariations = null;
            if (orderItemVariationsResponse.IsSuccessStatusCode)
            {
                var orderItemVariationsData = await orderItemVariationsResponse.Content.ReadAsStringAsync();
                orderItemVariations = JsonSerializer.Deserialize<List<OrderItemVariation>>(orderItemVariationsData, JsonOptions.Default);
            }
            else
            {
                orderItemVariations = new List<OrderItemVariation>();
            }
            
            var orderServicesApiUrl = ApiRoutes.Order.GetOrderServices(orderId);
            var orderServicesResponse = await _httpClient.GetAsync(orderServicesApiUrl);

            List<OrderService>? orderServices = null;
            if (orderServicesResponse.IsSuccessStatusCode)
            {
                var orderServicesData = await orderServicesResponse.Content.ReadAsStringAsync();
                orderServices = JsonSerializer.Deserialize<List<OrderService>>(orderServicesData,JsonOptions.Default);
            }
            else
            {
                orderServices = new List<OrderService>();
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
            foreach (var service in orderServices)
            {
                totalCharge += service.Charge;
                totalTax += service.Tax;
            }
            
            total = totalTax + totalCharge + order.TipAmount;

            var model = new ReceiptViewModel()
            {
                OrderId = orderId,
                EmployeeId = order.UserId,
                ClosedAt = order.ClosedAt,
                OrderItems = orderItems,
                OrderItemVariatons = orderItemVariations,
                OrderServices = orderServices,
                Total = total,
                TotalCharge = totalCharge,
                Tips = order.TipAmount,
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
