using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PosShared.Models;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class BusinessEntityConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);

        builder.Property(x => x.Type)
            .HasConversion(new EnumToStringConverter<BusinessType>());

        builder.HasMany(x => x.Users)
            .WithOne(u => u.Business)
            .HasForeignKey(u => u.BusinessId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(x => x.Services)
            .WithOne(s => s.Business)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(x => x.Discounts)
            .WithOne(d => d.Business)
            .HasForeignKey(d => d.BusinessId);

        builder.HasMany(x => x.Taxes)
            .WithOne(t => t.Business)
            .HasForeignKey(t => t.BusinessId);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Business)
            .HasForeignKey(i => i.BusinessId);
    }
}