using BookKeeper.Api.Clock;
using BookKeeper.Api.Database;
using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookKeeper.Api.Features.Statistics;

public static class CreateStatisticOfYear
{
    [DisallowConcurrentExecution]
    internal sealed class ProcessStatisticOfYear(
        ApplicationDbContext applicationDbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessStatisticOfYear> logger)
        : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Begin processing CreateStatisticOfYear");

            int targetYear = dateTimeProvider.UtcNow.Year - 1;

            logger.LogInformation(
                "Processing statistics for Year: {Year}",
                targetYear);

            Dictionary<string, decimal> incomeSumByUsers = await applicationDbContext.Incomes
                .Where(i => i.IncomeDateOnUtc.Year == targetYear)
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
                .Where(e => e.PaymentDateOnUtc.Year == targetYear)
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

            Dictionary<string, StatisticOfYear> existedStatistics = await applicationDbContext.StatisticsOfYears
                .Where(soy => soy.Year == targetYear)
                .ToDictionaryAsync(soy => soy.UserId, context.CancellationToken);

            List<StatisticOfYear> statisticsToAdd = [];
            foreach (string userId in existingUserIds)
            {
                decimal totalIncome = incomeSumByUsers.GetValueOrDefault(userId, 0);
                decimal totalExpend = expendSumByUsers.GetValueOrDefault(userId, 0);

                if (totalIncome > 0 || totalExpend > 0)
                {
                    if (existedStatistics.TryGetValue(userId, out StatisticOfYear? existedStatistic))
                    {
                        existedStatistic.UpdateAmounts(totalExpend, totalIncome);
                    }
                    else
                    {
                        var statisticOfYear = StatisticOfYear.Create(
                            year: targetYear,
                            totalExpendAmount: totalExpend,
                            totalIncomeAmount: totalIncome,
                            userId: userId);

                        statisticsToAdd.Add(statisticOfYear);
                    }
                }
            }

            if (statisticsToAdd.Count > 0)
            {
                await applicationDbContext.StatisticsOfYears.AddRangeAsync(
                    statisticsToAdd,
                    context.CancellationToken);
            }

            await applicationDbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "Finished processing CreateStatisticOfYear. Added: {AddedCount}, Updated: {UpdatedCount}",
                statisticsToAdd.Count,
                existedStatistics.Count);
        }
    }

    internal sealed class ConfigureCreateStatisticOfYearJob
        : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            string jobName = typeof(ProcessStatisticOfYear).FullName!;

            options
                .AddJob<ProcessStatisticOfYear>(configure => configure.WithIdentity(jobName))
                .AddTrigger(configure =>
                    configure
                        .ForJob(jobName)
                        .WithCronSchedule("0 0 3 1 1 ?"));
        }
    }
}
