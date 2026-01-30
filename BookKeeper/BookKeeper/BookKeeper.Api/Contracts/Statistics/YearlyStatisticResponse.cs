namespace BookKeeper.Api.Contracts.Statistics;

public sealed record YearlyStatisticResponse
{
    public required int Year { get; init; }
    public required decimal TotalExpendAmount { get; init; }
    public required decimal TotalIncomeAmount { get; init; }
    public required decimal SumAmount { get; init; }
}
