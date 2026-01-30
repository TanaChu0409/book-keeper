using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Expenditure> Expenditures { set; get; }
    public DbSet<Income> Incomes { set; get; }
    public DbSet<Label> Labels { set; get; }
    public DbSet<User> Users { set; get; }
    public DbSet<StatisticOfDate> StatisticsOfDates { set; get; }
    public DbSet<StatisticOfMonth> StatisticsOfMonths { set; get; }
    public DbSet<StatisticOfWeek> StatisticsOfWeeks { set; get; }
    public DbSet<StatisticOfYear> StatisticsOfYears { set; get; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
