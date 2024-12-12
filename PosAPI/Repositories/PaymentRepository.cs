using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            var order = await _context.Orders
                                       .Include(o => o.Payments)
                                       .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
            {
                throw new Exception($"Payment for Order with ID {payment.OrderId} is invalid. Order does not exist.");
            }

            try
            {
                payment.Order = order;

                await _context.Payments.AddAsync(payment);

                order.Payments.Add(payment);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding the new payment to the database.", ex);
            }
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Set<Payment>()
               .OrderBy(p => p.Id)
               .ToListAsync();
        }
        public async Task<List<Payment?>> GetAllOrderPaymentsAsync(int orderId)
        {
            var payments = await _context.Set<Payment>()
                     .Where(p => p.OrderId == orderId)
                     .OrderBy(tax => tax.Id)
                     .ToListAsync();

            return payments;
        }
        public async Task<List<Payment>> GetAllBusinessPaymentsAsync(int businessId)
        {
            return await _context.Set<Payment>()
                     .Where(p => p.Order.BusinessId == businessId)
                     .OrderBy(tax => tax.Id)
                     .ToListAsync();
        }
        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Set<Payment>().FindAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            }

            return payment;
        }

        public Task UpdatePaymentAsync(Payment payment)
        {
            throw new NotImplementedException();
        }

        public Task DeletePaymentAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
