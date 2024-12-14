using PosShared.Models;
using PosAPI.Data.DbContext.EntityConfigurations;
namespace PosAPI.Data.DbContext;

using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemVariation> ItemVariations { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Tax> Taxes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<DefaultShiftPattern> DefaultShiftPatterns { get; set; }
    public DbSet<TimeOff> TimeOffs { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Refund> Refunds { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderItemVariation> OrderItemVariations { get; set; }
    public DbSet<OrderItemTax> OrderItemTaxes { get; set; }
    public DbSet<OrderDiscount> OrderDiscounts { get; set; }
    public DbSet<OrderItemDiscount> OrderItemDiscounts { get; set; }

    public DbSet<OrderService> OrderServices { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Customer?> Customers { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new BusinessEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ItemEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ItemVariationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DiscountEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TaxEntityConfiguration());

        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

        modelBuilder.ApplyConfiguration(new ScheduleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DefaultShiftPatternEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TimeOffEntityConfiguration());

        modelBuilder.ApplyConfiguration(new OrderEntityConfiguration());

        modelBuilder.ApplyConfiguration(new PaymentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefundEntityConfigurations());

        modelBuilder.ApplyConfiguration(new OrderItemEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemVariationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemTaxEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrderDiscountEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemDiscountEntityConfiguration());

        modelBuilder.ApplyConfiguration(new OrderServiceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationEntityConfiguration());
    }
}