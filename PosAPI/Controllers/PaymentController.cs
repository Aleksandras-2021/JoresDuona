using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{

    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderController> _logger;
    public PaymentController(IPaymentService paymentService, IUserRepository userRepository, IPaymentRepository paymentRepository, ILogger<OrderController> logger)
    {
        _paymentService = paymentService;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }


    // GET: api/Payment
    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        User? sender = await GetUserFromToken();
        
        List<Payment> payments;
        if (sender.Role == UserRole.SuperAdmin)
        {
            payments = await _paymentRepository.GetAllPaymentsAsync();
        }
        else
        {
            payments = await _paymentRepository.GetAllBusinessPaymentsAsync(sender.BusinessId);
        }
        
        if (payments == null || payments.Count == 0)
        {
            return NotFound("No items found.");
        }

        return Ok(payments);
    }

    // GET: api/Payment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        User? sender = await GetUserFromToken();
        var payment = _paymentService.GetAuthorizedPaymentById(id, sender);
        return Ok(payment);
    }

    // GET: api/Payment/Order/{orderId}
    [HttpGet("Order/{orderId}")]
    public async Task<IActionResult> GetAllOrderPayments(int orderId)
    {
        User? senderUser = await GetUserFromToken();
        var payments = await _paymentService.GetAuthorizedOrderPayments(orderId,senderUser);
        return Ok(payments);
    }

    // POST: api/Payment
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] AddPaymentDTO payment)
    {
        User? sender = await GetUserFromToken();
        var newPayment = await _paymentService.CreateAuthorizedOrderPayment(payment, sender);
        return CreatedAtAction(nameof(GetPaymentById), new { id = newPayment.Id }, newPayment);
    }
    
    // POST: api/Payment/Refund
    [HttpPost("Refund/{paymentId}")]
    public async Task<IActionResult> CreatePayment([FromBody] RefundDTO refund,int paymentId)
    {
        User? sender = await GetUserFromToken();
        
        var refundPayment  = await _paymentService.CreateAuthorizedRefund(refund, sender);           

        return CreatedAtAction(nameof(GetPaymentById), new { id = refundPayment.Id }, refundPayment);
    }



    #region HelperMethods
    private async Task<User?> GetUserFromToken()
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization token is missing or null.");
            return null;
        }

        int? userId = Ultilities.ExtractUserIdFromToken(token);
        User? user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning($"Failed to find user with {userId} in DB");
            return null;
        }

        return user;

    }
    #endregion

}
