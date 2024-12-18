using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public class PaymentService: IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    
    
    public PaymentService(IPaymentRepository paymentRepository,IOrderService orderService, IReservationRepository reservationRepository
        ,IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderService = orderService;
        _orderRepository = orderRepository;
    }

    public async Task<List<Payment>> GetAllPayments(User? sender)
    {
        AuthorizationHelper.Authorize("Payment", "List", sender);
        
        List<Payment> payments;

        if (sender.Role == UserRole.SuperAdmin)
        {
            payments = await _paymentRepository.GetAllPaymentsAsync();
        }
        else
        {
            payments = await _paymentRepository.GetAllBusinessPaymentsAsync(sender.BusinessId);
        }

        return payments;
    }

    
    
    public async Task<List<Payment?>> GetAuthorizedOrderPayments(int orderId, User? sender)
    {
        AuthorizationHelper.Authorize("Payment", "List", sender);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "List");
        var payments = await _paymentRepository.GetAllOrderPaymentsAsync(orderId);

        return payments;
    }

    public async Task<Payment> CreateAuthorizedOrderPayment(AddPaymentDTO payment, User? sender)
    {
        AuthorizationHelper.Authorize("Payment", "Create", sender);
        Order? order = await _orderRepository.GetOrderByIdWithPaymentsAsync(payment.OrderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "List");

        if (payment.Amount < 0)
            throw new BusinessRuleViolationException("Cannot make a negative payment");

        DateTime now = DateTime.Now.ToUniversalTime();
        
        var newPayment = new Payment()
        {
            OrderId = payment.OrderId,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = now,
            Amount = payment.Amount,
            PaymentGateway = PaymentGateway.Stripe, // Always Stripe
            TransactionId = null,
        };
        await _paymentRepository.AddPaymentAsync(newPayment);

        decimal sum = await _paymentRepository.GetTotalPaymentsForOrderAsync(newPayment.OrderId);

        if (sum == order.ChargeAmount + order.TaxAmount)
        {
            order.Status = OrderStatus.Paid;
        }
        else if (sum < order.ChargeAmount + order.TaxAmount)
        {
            order.Status = OrderStatus.PartiallyPaid;
        }
        else if (sum > order.ChargeAmount + order.TaxAmount)
        {
            order.TipAmount = sum - order.ChargeAmount - order.TaxAmount;
            order.Status = OrderStatus.Paid;
        }

        await _orderRepository.UpdateOrderAsync(order);

        return newPayment;
    }

    public async Task<Payment> CreateAuthorizedRefund(RefundDTO refund, User? sender)
    {
        AuthorizationHelper.Authorize("Payment", "Create", sender);
        var payment = await _paymentRepository.GetPaymentByIdAsync(refund.PaymentId);
        var order = await _orderRepository.GetOrderByIdAsync(payment.OrderId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "List");
        
        if (refund.Amount < 0)
        {
            throw new BusinessRuleViolationException("Refund must be higher than 0");
        }

        DateTime now = DateTime.Now.ToUniversalTime();
        
        //refund  is a negative payment
        var newPayment = new Payment()
        {
            OrderId = payment.OrderId,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = now,
            Amount = payment.Amount * (-1),
            PaymentGateway = PaymentGateway.Stripe, // Always Stripe
        };
        
        await _paymentRepository.AddPaymentAsync(newPayment);
        order.Status = OrderStatus.Refunded;
        order.ClosedAt ??= DateTime.UtcNow.AddHours(2);
        
        await _orderRepository.UpdateOrderAsync(order);
        
        return newPayment;
    }


    public async Task<Payment> GetAuthorizedPaymentById(int id, User? sender)
    {
        AuthorizationHelper.Authorize("Payment", "Read", sender);
        var payment = await _paymentRepository.GetPaymentByIdAsync(id);        
        var order = await _orderRepository.GetOrderByIdAsync(payment.OrderId);        
        AuthorizationHelper.ValidateOwnershipOrRole(sender, order.BusinessId, sender.BusinessId, "List");
        
        return payment;
    }
}