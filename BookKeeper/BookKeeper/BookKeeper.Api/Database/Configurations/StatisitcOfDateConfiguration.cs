using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class StatisitcOfDateConfiguration : IEntityTypeConfiguration<StatisticOfDate>
{
    public void Configure(EntityTypeBuilder<StatisticOfDate> builder)
    {
        builder.HasKey(sod => sod.Id);

        builder.Property(sod => sod.Id).HasMaxLength(500);
        builder.Property(sod => sod.UserId).HasMaxLength(500);
        builder.Property(sod => sod.TotalExpendAmount).HasPrecision(18, 0);
        builder.Property(sod => sod.TotalIncomeAmount).HasPrecision(18, 0);
        builder.Property(sod => sod.SumAmount).HasPrecision(18, 0);

        builder.HasIndex(sod => new { sod.UserId, sod.DateOnUtc })
            .IsUnique()
            .HasDatabaseName("ix_statistics_of_date_user_id_date_on_utc");
    }
}
