namespace BookKeeper.Api.Entities;

public class StatisticOfDate
{
    private StatisticOfDate()
    {
    }

    public string Id { get; private set; }
    public DateOnly DateOnUtc { get; private set; }
    public decimal TotalExpendAmount { get; private set; }
    public decimal TotalIncomeAmount { get; private set; }
    public decimal SumAmount { get; private set; }
    public string UserId { get; private set; }

    public static StatisticOfDate Create(
        DateOnly dateOnUtc,
        decimal totalExpendAmount,
        decimal totalIncomeAmount,
        string userId) =>
        new()
        {
            Id = $"sod_{Ulid.NewUlid()}",
            DateOnUtc = dateOnUtc,
            TotalExpendAmount = totalExpendAmount,
            TotalIncomeAmount = totalIncomeAmount,
            SumAmount = totalIncomeAmount - totalExpendAmount,
            UserId = userId
        };

    public void UpdateAmounts(decimal totalExpendAmount, decimal totalIncomeAmount)
    {
        TotalExpendAmount = totalExpendAmount;
        TotalIncomeAmount = totalIncomeAmount;
        SumAmount = totalIncomeAmount - totalExpendAmount;
    }
}
