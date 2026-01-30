using BookKeeper.Api.Clock;
using BookKeeper.Api.Database;
using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using System.Globalization;

namespace BookKeeper.Api.Features.Statistics;

public static class CreateStatisticOfWeek
{
    [DisallowConcurrentExecution]
    internal sealed class ProcessStatisticOfWeek(
        ApplicationDbContext applicationDbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessStatisticOfWeek> logger)
        : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Begin processing CreateStatisticOfWeek");

            // 處理上週資料（使用台灣時區）
            DateTime lastWeek = dateTimeProvider.TaipeiNow.AddDays(-7);
            DateTime weekStart = GetStartOfWeek(lastWeek);
            DateTime weekEnd = weekStart.AddDays(7).AddSeconds(-1);

            // 計算年月與當月第幾週
            int targetYear = weekStart.Year;
            int targetMonth = weekStart.Month;
            int targetWeekOfMonth = GetWeekOfMonth(weekStart);

            logger.LogInformation(
                "Processing statistics for Year: {Year}, Month: {Month}, Week: {WeekOfMonth} (from {StartDate} to {EndDate})",
                targetYear,
                targetMonth,
                targetWeekOfMonth,
                DateOnly.FromDateTime(weekStart),
                DateOnly.FromDateTime(weekEnd));

            var startDate = DateOnly.FromDateTime(weekStart);
            var endDate = DateOnly.FromDateTime(weekEnd);

            Dictionary<string, decimal> incomeSumByUsers = await applicationDbContext.Incomes
                .Where(i => i.IncomeDateOnUtc >= startDate && i.IncomeDateOnUtc <= endDate)
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
                .Where(e => e.PaymentDateOnUtc >= startDate && e.PaymentDateOnUtc <= endDate)
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

            Dictionary<string, StatisticOfWeek> existedStatistics = await applicationDbContext.StatisticsOfWeeks
                .Where(sow => sow.Year == targetYear && sow.Month == targetMonth && sow.WeekOfMonth == targetWeekOfMonth)
                .ToDictionaryAsync(sow => sow.UserId, context.CancellationToken);

            List<StatisticOfWeek> statisticsToAdd = [];
            foreach (string userId in existingUserIds)
            {
                decimal totalIncome = incomeSumByUsers.GetValueOrDefault(userId, 0);
                decimal totalExpend = expendSumByUsers.GetValueOrDefault(userId, 0);

                // 即使當周無紀錄也要記錄 0（確保所有使用者都有完整的週統計歷史）
                if (existedStatistics.TryGetValue(userId, out StatisticOfWeek? existedStatistic))
                {
                    existedStatistic.UpdateAmounts(totalExpend, totalIncome);
                }
                else
                {
                    var statisticOfWeek = StatisticOfWeek.Create(
                        year: targetYear,
                        month: targetMonth,
                        weekOfMonth: targetWeekOfMonth,
                        totalExpendAmount: totalExpend,
                        totalIncomeAmount: totalIncome,
                        userId: userId);

                    statisticsToAdd.Add(statisticOfWeek);
                }
            }

            if (statisticsToAdd.Count > 0)
            {
                await applicationDbContext.StatisticsOfWeeks.AddRangeAsync(
                    statisticsToAdd,
                    context.CancellationToken);
            }

            await applicationDbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "Finished processing CreateStatisticOfWeek. Added: {AddedCount}, Updated: {UpdatedCount}",
                statisticsToAdd.Count,
                existedStatistics.Count);
        }

        /// <summary>
        /// 取得該週的週一（ISO 8601 標準）
        /// </summary>
        private static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        /// <summary>
        /// 計算該日期是當月的第幾週（從 1 開始計數）
        /// 以該月第一個週一作為第 1 週的起始
        /// </summary>
        private static int GetWeekOfMonth(DateTime date)
        {
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var firstDayOfMonth = new DateTime(utcDate.Year, utcDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime firstMondayOfMonth = GetStartOfWeek(firstDayOfMonth.AddDays(6));
            // 如果該日期在第一個完整週之前，屬於第 0 週（上月尾週）
            if (utcDate < firstMondayOfMonth)
            {
                return 0;
            }
            int daysSinceFirstMonday = (utcDate - firstMondayOfMonth).Days;
            return daysSinceFirstMonday / 7 + 1;
        }
    }

    internal sealed class ConfigureCreateStatisticOfWeekJob
        : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            string jobName = typeof(ProcessStatisticOfWeek).FullName!;

            options
                .AddJob<ProcessStatisticOfWeek>(configure => configure.WithIdentity(jobName))
                .AddTrigger(configure =>
                    configure
                        .ForJob(jobName)
                        .WithCronSchedule("0 0 19 ? * SUN"));  // UTC Sunday 19:00 = Taiwan Monday
        }
    }
}
