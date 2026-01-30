namespace BookKeeper.Api.Contracts.Statistics;

public sealed record DailyStatisticResponse
{
    public required DateOnly Date { get; init; }
    public required decimal TotalExpendAmount { get; init; }
    public required decimal TotalIncomeAmount { get; init; }
    public required decimal SumAmount { get; init; }
}
