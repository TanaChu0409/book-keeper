using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasMaxLength(500);
        builder.Property(i => i.IncomeName).HasMaxLength(500);
        builder.Property(i => i.LabelId).HasMaxLength(500);
        builder.Property(i => i.Amount).HasPrecision(18, 0);
        
        builder.HasOne(i => i.Label)
            .WithMany()
            .HasForeignKey(i => i.LabelId);
    }
}
