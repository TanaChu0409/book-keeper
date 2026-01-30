using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class StatisticOfYearConfiguration : IEntityTypeConfiguration<StatisticOfYear>
{
    public void Configure(EntityTypeBuilder<StatisticOfYear> builder)
    {
        builder.HasKey(soy => soy.Id);

        builder.Property(soy => soy.Id).HasMaxLength(500);
        builder.Property(soy => soy.UserId).HasMaxLength(500);
        builder.Property(soy => soy.TotalExpendAmount).HasPrecision(18, 0);
        builder.Property(soy => soy.TotalIncomeAmount).HasPrecision(18, 0);
        builder.Property(soy => soy.SumAmount).HasPrecision(18, 0);

        builder.HasIndex(soy => new { soy.UserId, soy.Year })
            .IsUnique()
            .HasDatabaseName("ix_statistics_of_year_user_id_year");
    }
}
