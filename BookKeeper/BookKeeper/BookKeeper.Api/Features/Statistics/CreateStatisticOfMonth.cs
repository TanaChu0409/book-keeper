using BookKeeper.Api.Clock;
using BookKeeper.Api.Database;
using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookKeeper.Api.Features.Statistics;

public static class CreateStatisticOfMonth
{
    [DisallowConcurrentExecution]
    internal sealed class ProcessStatisticOfMonth(
        ApplicationDbContext applicationDbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessStatisticOfMonth> logger)
        : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Begin processing CreateStatisticOfMonth");

            DateTime lastMonth = dateTimeProvider.UtcNow.AddMonths(-1);
            int targetYear = lastMonth.Year;
            int targetMonth = lastMonth.Month;

            logger.LogInformation(
                "Processing statistics for Year: {Year}, Month: {Month}",
                targetYear,
                targetMonth);

            Dictionary<string, decimal> incomeSumByUsers = await applicationDbContext.Incomes
                .Where(i => i.IncomeDateOnUtc.Year == targetYear && i.IncomeDateOnUtc.Month == targetMonth)
                .GroupBy(i => i.UserId)
                .Select(g =>
                    new
                    {
                        UserId = g.Key,
                        TotalIncome = g.Sum(i => i.Amount)
                    })
                .ToDictionaryAsync(
                    x => x.UserId,
                    x => x.TotalIncome,
                    context.CancellationToken);

            Dictionary<string, decimal> expendSumByUsers = await applicationDbContext.Expenditures
                .Where(e => e.PaymentDateOnUtc.Year == targetYear && e.PaymentDateOnUtc.Month == targetMonth)
                .GroupBy(e => e.UserId)
                .Select(g =>
                    new
                    {
                        UserId = g.Key,
                        TotalExpend = g.Sum(e => e.Amount)
                    })
                .ToDictionaryAsync(
                    x => x.UserId,
                    x => x.TotalExpend,
                    context.CancellationToken);

            List<string> existingUserIds = await applicationDbContext.Users
                .Select(u => u.Id)
                .ToListAsync(context.CancellationToken);

            Dictionary<string, StatisticOfMonth> existedStatistics = await applicationDbContext.StatisticsOfMonths
                .Where(som => som.Year == targetYear && som.Month == targetMonth)
                .ToDictionaryAsync(som => som.UserId, context.CancellationToken);

            List<StatisticOfMonth> statisticsToAdd = [];
            foreach (string userId in existingUserIds)
            {
                decimal totalIncome = incomeSumByUsers.GetValueOrDefault(userId, 0);
                decimal totalExpend = expendSumByUsers.GetValueOrDefault(userId, 0);

                if (totalIncome > 0 || totalExpend > 0)
                {
                    if (existedStatistics.TryGetValue(userId, out StatisticOfMonth? existedStatistic))
                    {
                        existedStatistic.UpdateAmounts(totalExpend, totalIncome);
                    }
                    else
                    {
                        var statisticOfMonth = StatisticOfMonth.Create(
                            year: targetYear,
                            month: targetMonth,
                            totalExpendAmount: totalExpend,
                            totalIncomeAmount: totalIncome,
                            userId: userId);

                        statisticsToAdd.Add(statisticOfMonth);
                    }
                }
            }

            if (statisticsToAdd.Count > 0)
            {
                await applicationDbContext.StatisticsOfMonths.AddRangeAsync(
                    statisticsToAdd,
                    context.CancellationToken);
            }

            await applicationDbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "Finished processing CreateStatisticOfMonth. Added: {AddedCount}, Updated: {UpdatedCount}",
                statisticsToAdd.Count,
                existedStatistics.Count);
        }
    }

    internal sealed class ConfigureCreateStatisticOfMonthJob
        : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            string jobName = typeof(ProcessStatisticOfMonth).FullName!;

            options
                .AddJob<ProcessStatisticOfMonth>(configure => configure.WithIdentity(jobName))
                .AddTrigger(configure =>
                    configure
                        .ForJob(jobName)
                        .WithCronSchedule("0 0 3 1 * ?"));
        }
    }
}
