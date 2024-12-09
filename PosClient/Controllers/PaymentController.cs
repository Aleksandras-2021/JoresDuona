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


}
