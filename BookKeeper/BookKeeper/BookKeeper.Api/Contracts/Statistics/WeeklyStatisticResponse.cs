namespace BookKeeper.Api.Contracts.Statistics;

public sealed record WeeklyStatisticResponse
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required int WeekOfMonth { get; init; }
    public required decimal TotalExpendAmount { get; init; }
    public required decimal TotalIncomeAmount { get; init; }
    public required decimal SumAmount { get; init; }
}
