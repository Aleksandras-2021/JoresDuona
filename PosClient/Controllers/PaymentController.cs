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
        // Handle the payment creation logic here
        string token = Request.Cookies["authToken"];
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
}
