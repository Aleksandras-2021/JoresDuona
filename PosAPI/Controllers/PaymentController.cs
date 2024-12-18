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

    private readonly IUserTokenService _userTokenService;
    private readonly IPaymentService _paymentService;
    public PaymentController(IPaymentService paymentService, IUserTokenService userTokenService)
    {
        _paymentService = paymentService;
        _userTokenService = userTokenService;
    }


    // GET: api/Payment
    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();

        var payments = await _paymentService.GetAllPayments(sender);
        
        return Ok(payments);
    }

    // GET: api/Payment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var payment = _paymentService.GetAuthorizedPaymentById(id, sender);
        return Ok(payment);
    }

    // GET: api/Payment/Order/{orderId}
    [HttpGet("Order/{orderId}")]
    public async Task<IActionResult> GetAllOrderPayments(int orderId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var payments = await _paymentService.GetAuthorizedOrderPayments(orderId,sender);
        return Ok(payments);
    }

    // POST: api/Payment
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] AddPaymentDTO payment)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var newPayment = await _paymentService.CreateAuthorizedOrderPayment(payment, sender);
        return CreatedAtAction(nameof(GetPaymentById), new { id = newPayment.Id }, newPayment);
    }
    
    // POST: api/Payment/Refund
    [HttpPost("Refund/{paymentId}")]
    public async Task<IActionResult> CreatePayment([FromBody] RefundDTO refund,int paymentId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var refundPayment  = await _paymentService.CreateAuthorizedRefund(refund, sender);           
        return CreatedAtAction(nameof(GetPaymentById), new { id = refundPayment.Id }, refundPayment);
    }
}
