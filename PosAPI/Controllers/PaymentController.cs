using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{

    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITaxRepository _taxRepository;
    private readonly ILogger<OrderController> _logger;
    public PaymentController(IOrderRepository orderRepository, IUserRepository userRepository, IPaymentRepository paymentRepository, ITaxRepository taxRepository, ILogger<OrderController> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _taxRepository = taxRepository;
        _logger = logger;
    }


    // GET: api/Payment
    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all payments: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Payment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        User? senderUser = await GetUserFromToken();

        if (senderUser == null)
            return Unauthorized();

        try
        {
            Payment? payment;

            if (senderUser.Role == UserRole.SuperAdmin)
            {
                payment = await _paymentRepository.GetPaymentByIdAsync(id);
            }
            else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
            {
                payment = await _paymentRepository.GetPaymentByIdAsync(id);

                if (payment.Order.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }

            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found.");
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving Payment with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Payment/Order/{orderId}
    [HttpGet("Order/{orderId}")]
    public async Task<IActionResult> GetAllOrderPayments(int orderId)
    {
        User? senderUser = await GetUserFromToken();

        if (senderUser == null)
            return Unauthorized();

        try
        {
            List<Payment?> payments;

            if (senderUser.Role == UserRole.SuperAdmin)
            {
                payments = await _paymentRepository.GetAllOrderPaymentsAsync(orderId);
            }
            else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
            {
                Order order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }
                payments = await _paymentRepository.GetAllOrderPaymentsAsync(orderId);

            }
            else
            {
                return Unauthorized();
            }

            if (payments == null)
            {
                return NotFound($"Payment with for order with ID {orderId} not found.");
            }

            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving order with ID {orderId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Payment
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] AddPaymentDTO payment)
    {
        User? sender = await GetUserFromToken();
        Order order = await _orderRepository.GetOrderByIdAsync(payment.OrderId);

        _logger.LogInformation($"{sender.Name} is sending a payment to an order {payment.OrderId}");

        if (payment == null)
            return BadRequest("Payment data is null.");

        if (sender == null || sender.BusinessId != order.BusinessId)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        Payment newPayment = new Payment();

        newPayment.OrderId = payment.OrderId;
        newPayment.Order = order;
        newPayment.PaymentMethod = payment.PaymentMethod;
        newPayment.PaymentDate = DateTime.UtcNow.AddHours(2);;
        newPayment.Amount = payment.Amount;
        newPayment.PaymentGateway = PaymentGateway.Stripe; //Its always this , no point in changing
        newPayment.TransactionId = null;

        try
        {
            await _paymentRepository.AddPaymentAsync(newPayment);

            List<Payment> payments = await _paymentRepository.GetAllOrderPaymentsAsync(newPayment.OrderId);
            decimal sum = 0;

            foreach (var orderPayment in payments)
            {
                sum += orderPayment.Amount;
            }

            if (sum == order.ChargeAmount + order.TaxAmount)
            {
                order.Status = OrderStatus.Closed;
                order.ClosedAt = DateTime.UtcNow.AddHours(2);;
            }
            else if (sum < order.ChargeAmount + order.TaxAmount)
                order.Status = OrderStatus.PartiallyPaid;
            else if (sum > order.ChargeAmount + order.TaxAmount)
            {
                order.TipAmount = sum - order.ChargeAmount - order.TaxAmount;
                order.ClosedAt = DateTime.UtcNow.AddHours(2);
                order.Status = OrderStatus.Closed;
            }

            _logger.LogInformation($"Payment of {payment.Amount}/{order.ChargeAmount} euros has been made to an order with id({payment.OrderId}) order status now is ({order.Status})");
           
            await _orderRepository.UpdateOrderAsync(order);

            return CreatedAtAction(nameof(GetPaymentById), new { id = newPayment.Id }, newPayment);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
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
