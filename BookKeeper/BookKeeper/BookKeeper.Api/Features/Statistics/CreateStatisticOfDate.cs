using BookKeeper.Api.Database;
using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookKeeper.Api.Features.Statistics;

public static class CreateStatisticOfDate
{
    [DisallowConcurrentExecution]
    internal sealed class ProcessStatisticOfDate(
        ApplicationDbContext applicationDbContext,
        ILogger<ProcessStatisticOfDate> logger)
        : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Begin processing CreateStatisticOfDate");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            Dictionary<string, decimal> incomeSumByUsers = await applicationDbContext.Incomes
                .Where(i => i.IncomeDateOnUtc == today)
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
                .Where(e => e.PaymentDateOnUtc == today)
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

            Dictionary<string, StatisticOfDate> existedStatistics = await applicationDbContext.StatisticsOfDates
                .Where(sod => sod.DateOnUtc == today)
                .ToDictionaryAsync(sod => sod.UserId, context.CancellationToken);

            List<StatisticOfDate> statisticsToAdd = [];
            foreach (string userId in existingUserIds)
            {
                decimal totalIncome = incomeSumByUsers.GetValueOrDefault(userId, 0);
                decimal totalExpend = expendSumByUsers.GetValueOrDefault(userId, 0);
                if (existedStatistics.TryGetValue(userId, out StatisticOfDate existedStatistic))
                {
                    existedStatistic.UpdateAmounts(totalExpend, totalIncome);
                }
                else
                {
                    var statisticOfDate = StatisticOfDate.Create(
                        dateOnUtc: today,
                        totalExpendAmount: totalExpend,
                        totalIncomeAmount: totalIncome,
                        userId: userId);

                    statisticsToAdd.Add(statisticOfDate);
                }
            }

            if (statisticsToAdd.Count > 0)
            {
                await applicationDbContext.StatisticsOfDates.AddRangeAsync(
                    statisticsToAdd,
                    context.CancellationToken);
            }

            await applicationDbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Finished processing CreateStatisticOfDate");
        }
    }

    internal sealed class ConfigureCreateStatisticOfDateJob
        : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            string jobName = typeof(ProcessStatisticOfDate).FullName!;

            options
                .AddJob<ProcessStatisticOfDate>(configure => configure.WithIdentity(jobName))
                .AddTrigger(configure =>
                    configure
                        .ForJob(jobName)
                        .WithDailyTimeIntervalSchedule(schedule =>
                            schedule
                                .OnEveryDay()
                                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(3, 0))));
        }
    }
}
