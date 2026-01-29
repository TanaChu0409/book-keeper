using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class StatisticOfWeekConfiguration : IEntityTypeConfiguration<StatisticOfWeek>
{
    public void Configure(EntityTypeBuilder<StatisticOfWeek> builder)
    {
        builder.HasKey(sow => sow.Id);

        builder.Property(sow => sow.Id).HasMaxLength(500);
        builder.Property(sow => sow.UserId).HasMaxLength(500);
        builder.Property(sow => sow.TotalExpendAmount).HasPrecision(18, 0);
        builder.Property(sow => sow.TotalIncomeAmount).HasPrecision(18, 0);
        builder.Property(sow => sow.SumAmount).HasPrecision(18, 0);

        builder.HasIndex(sow => new { sow.UserId, sow.Year, sow.Month, sow.WeekOfMonth })
            .IsUnique()
            .HasDatabaseName("ix_statistics_of_week_user_id_year_month_week");
    }
}
