using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class StatisticOfMonthConfiguration : IEntityTypeConfiguration<StatisticOfMonth>
{
    public void Configure(EntityTypeBuilder<StatisticOfMonth> builder)
    {
        builder.HasKey(som => som.Id);

        builder.Property(som => som.Id).HasMaxLength(500);
        builder.Property(som => som.UserId).HasMaxLength(500);
        builder.Property(som => som.TotalExpendAmount).HasPrecision(18, 0);
        builder.Property(som => som.TotalIncomeAmount).HasPrecision(18, 0);
        builder.Property(som => som.SumAmount).HasPrecision(18, 0);

        builder.HasIndex(som => new { som.UserId, som.Year, som.Month })
            .IsUnique()
            .HasDatabaseName("ix_statistics_of_month_user_id_year_month");
    }
}