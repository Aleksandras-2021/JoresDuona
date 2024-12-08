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

            Order order = await _context.Orders.FindAsync(payment.OrderId);

            if (order == null)
            {
                throw new Exception($" Payment for Order with ID {payment.OrderId} is invalid, Order does not exist.");
            }
            try
            {
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
        public async Task<List<Payment>> GetAllOrderPaymentsAsync(int orderId)
        {
            return await _context.Set<Payment>()
                     .Where(p => p.OrderId == orderId)
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

        public async Task UpdateTaxAsync(Tax tax)
        {
            if (tax == null)
            {
                throw new ArgumentNullException(nameof(tax));
            }

            var existingTax = await _context.Set<Tax>().FindAsync(tax.Id);
            if (existingTax == null)
            {
                throw new KeyNotFoundException($"Tax with ID {tax.Id} not found.");
            }

            existingTax.Id = tax.Id;
            existingTax.BusinessId = tax.BusinessId;
            existingTax.Business = tax.Business;
            existingTax.Name = tax.Name;
            existingTax.Amount = tax.Amount;
            existingTax.IsPercentage = tax.IsPercentage;
            existingTax.Category = tax.Category;

            if (existingTax.Business == null)
                existingTax.Business = await _context.Businesses.FindAsync(tax.BusinessId);



            _context.Set<Tax>().Update(existingTax);
            await _context.SaveChangesAsync();
        }
    }
}
