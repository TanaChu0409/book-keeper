using BookKeeper.Api.Database;
using Quartz;

namespace BookKeeper.Api.Features.Statistics;

public static class CreateStatisticOfDate
{
    internal sealed class ProcessStatisticOfDate(
        ApplicationDbContext applicationDbContext,
        ILogger<ProcessStatisticOfDate> logger)
        : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Begin processing CreateStatisticOfDate");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // get this date income data and group by user id
            var incomeByUserIds = applicationDbContext.Incomes
                .Where(i => i.IncomeDateOnUtc == today)
                .GroupBy(i => i.UserId)
                .ToList();
            // get this date expenditure data and group by user id
            var expenditureByUserIds = applicationDbContext.Expenditures
                .Where(e => e.PaymentDateOnUtc == today)
                .GroupBy(e => e.UserId)
                .ToList();

            throw new NotImplementedException();
        }
    }
}
