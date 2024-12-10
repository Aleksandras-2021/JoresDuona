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

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Get Order Items
        var orderItemsApiUrl = _apiUrl + $"/api/Order/{orderId}/Items";
        var response = await _httpClient.GetAsync(orderItemsApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to fetch order Items.";
        }

        var orderItemsData = await response.Content.ReadAsStringAsync();
        List<OrderItem>? orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Get Order Variations
        var orderItemVariationsApiUrl = _apiUrl + $"/api/Order/{orderId}/Variations";
        response = await _httpClient.GetAsync(orderItemVariationsApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to fetch order item variations.";
        }

        var orderItemsVariationsData = await response.Content.ReadAsStringAsync();
        List<OrderItemVariation>? orderItemsVariations = JsonSerializer.Deserialize<List<OrderItemVariation>>(orderItemsVariationsData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Get all Taxes

        var taxsApiUrl = _apiUrl + $"/api/Tax";
        response = await _httpClient.GetAsync(taxsApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to fetch taxes for order.";
        }

        var taxData = await response.Content.ReadAsStringAsync();
        List<Tax>? taxes = JsonSerializer.Deserialize<List<Tax>>(taxData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        ReceiptViewModel model = new ReceiptViewModel()
        {
            OrderId = orderId,
            OrderItems = orderItems,
            OrderItemVariatons = orderItemsVariations,
            Taxes = taxes
        };


        return View("Receipt", model);
    }


}
