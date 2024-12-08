using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IPaymentRepository
    {
        Task AddPaymentAsync(Payment payment);
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<List<Payment>> GetAllOrderPaymentsAsync(int orderId);
        Task<List<Payment>> GetAllBusinessPaymentsAsync(int businessId);

        Task<Payment> GetPaymentByIdAsync(int id);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
    }
}
