namespace BookKeeper.Api.Entities;

public class StatisticOfMonth
{
    private StatisticOfMonth()
    {
    }

    public string Id { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal TotalExpendAmount { get; private set; }
    public decimal TotalIncomeAmount { get; private set; }
    public decimal SumAmount { get; private set; }
    public string UserId { get; private set; }

    public static StatisticOfMonth Create(
        int year,
        int month,
        decimal totalExpendAmount,
        decimal totalIncomeAmount,
        string userId) =>
        new()
        {
            Id = $"som_{Ulid.NewUlid()}",
            Year = year,
            Month = month,
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
