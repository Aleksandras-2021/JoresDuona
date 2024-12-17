using Microsoft.AspNetCore.Mvc;
using PosClient.Services;
using PosShared.DTOs;
using PosShared.Models;
using PosShared;
using System.Text.Json;
using System.Text;
using PosShared.ViewModels;

namespace PosClient.Controllers;

public class PaymentController : Controller
{
    private readonly string _apiUrl = ApiRoutes.ApiBaseUrl;
    private readonly ApiService _apiService;

    public PaymentController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Create(int orderId, decimal untaxedAmount, decimal tax)
    {
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
        AddPaymentDTO payment = new AddPaymentDTO()
        {
            Amount = paymentViewModel.Amount,
            OrderId = paymentViewModel.OrderId,
            PaymentMethod = paymentViewModel.PaymentMethod
        };

        var content = new StringContent(JsonSerializer.Serialize(payment), Encoding.UTF8, "application/json");
        var response = await _apiService.PostAsync(ApiRoutes.Payment.Create, content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = $"Payment successfully added";
            return RedirectToAction("Index", "Order");
        }

        TempData["Error"] = "Failed to create Payment. Please try again. \n" + response.StatusCode;
        return View(paymentViewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> Refund(int orderId, decimal untaxedAmount, decimal tax)
    {
        var apiUrl = ApiRoutes.Order.GetOrderPayments(orderId);
        var response = await _apiService.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var paymentData = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment>>(paymentData,JsonOptions.Default);

            if (payments != null && payments.Any())
            {
                DateTime today = DateTime.Today;
                today = DateTime.SpecifyKind(today, DateTimeKind.Utc);

                var model = new RefundViewModel() 
                {
                    Payments = payments.Where(p => p.Amount > 0).ToList(),
                    RefundDate = today,
                    Amount = untaxedAmount + tax,
                    Reason = string.Empty,
                };

                return View("Refund", model);
            }
        }

        TempData["Error"] = "Failed to get Payments. Please try again. \n" + response.StatusCode;

        return RedirectToAction("Index", "Order");
    }

    [HttpPost]
    public async Task<IActionResult> Refund(RefundViewModel model,int paymentId)
    {
        var refund = new RefundDTO()
        {
            Amount = model.Amount,
            PaymentId = paymentId,
            RefundDate = model.RefundDate,
            Reason = model.Reason
        };

        var content = new StringContent(JsonSerializer.Serialize(refund), Encoding.UTF8, "application/json");
        var response = await _apiService.PostAsync(ApiRoutes.Payment.CreateRefund(paymentId), content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = $"Payment {paymentId} successfully refunded";
            return RedirectToAction("Index", "Order");
        }

        TempData["Error"] = "Failed to create Payment. \n" + response.StatusCode;
        return RedirectToAction("Refund");
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderPayments(int orderId)
    {
        var apiUrl = ApiRoutes.Order.GetOrderPayments(orderId);
        var response = await _apiService.GetAsync(apiUrl);

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
        try
        {
            // Get Order Items
            var orderApiUrl = ApiRoutes.Order.GetById(orderId);
            var orderResponse = await _apiService.GetAsync(orderApiUrl);
            Order? order = null;

            if (orderResponse.IsSuccessStatusCode)
            {
                var orderData = await orderResponse.Content.ReadAsStringAsync();
                order = JsonSerializer.Deserialize<Order>(orderData, JsonOptions.Default);
            }
            else
            {
                TempData["Error"] = "Couldn't retrieve order." + orderResponse.StatusCode;

            }
            
            // Get Order Items
            var orderItemsApiUrl = ApiRoutes.OrderItems.GetOrderItems(orderId);
            var orderItemsResponse = await _apiService.GetAsync(orderItemsApiUrl);

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
            var orderItemVariationsResponse = await _apiService.GetAsync(orderItemVariationsApiUrl);

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
            var orderServicesResponse = await _apiService.GetAsync(orderServicesApiUrl);

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
            
            decimal total = order.ChargeAmount + order.TaxAmount + order.TipAmount;
            
            var model = new ReceiptViewModel()
            {
                OrderId = orderId,
                EmployeeId = order.UserId,
                ClosedAt = order.ClosedAt,
                OrderItems = orderItems,
                OrderItemVariatons = orderItemVariations,
                OrderServices = orderServices,
                Total = total,
                TotalCharge = order.ChargeAmount,
                Tips = order.TipAmount,
                TotalTax = order.TaxAmount,
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
