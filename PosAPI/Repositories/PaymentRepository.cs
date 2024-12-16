using Microsoft.EntityFrameworkCore;
using PosAPI.Data.DbContext;
using PosAPI.Middlewares;
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
            ArgumentNullException.ThrowIfNull(payment);

            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
            {
                throw new BusinessRuleViolationException($"Payment for Order with ID {payment.OrderId} is invalid. Order does not exist.");
            }

            try
            {
                payment.Order = order;

                await _context.Payments.AddAsync(payment);

                order.Payments?.Add(payment);


                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("An error occurred while adding the new payment to the database.", ex);
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

        public async Task UpdatePaymentAsync(Payment payment)
        {
            throw new NotImplementedException();
        }

        public async Task DeletePaymentAsync(int id)
        {
            throw new NotImplementedException();
        }
        
        public async Task<decimal> GetTotalPaymentsForOrderAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .SumAsync(p => p.Amount);
        }

    }
}
