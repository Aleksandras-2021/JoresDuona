using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IPaymentService
{
    Task<List<Payment?>> GetAuthorizedOrderPayments(int orderId, User? sender);
    Task<Payment> CreateAuthorizedOrderPayment(AddPaymentDTO payment, User? sender);
    Task<Payment> GetAuthorizedPaymentById(int id, User? sender);
    Task<Payment> CreateAuthorizedRefund(RefundDTO refund, User? sender);


}