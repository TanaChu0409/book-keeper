using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class ExpenditureConfiguration : IEntityTypeConfiguration<Expenditure>
{
    public void Configure(EntityTypeBuilder<Expenditure> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(500);
        builder.Property(e => e.PaymentName).HasMaxLength(500);
        builder.Property(e => e.LabelId).HasMaxLength(500);
        builder.Property(e => e.Amount).HasPrecision(18, 0);

        builder.HasOne(e => e.Label)
            .WithMany()
            .HasForeignKey(e => e.LabelId);
    }
}
