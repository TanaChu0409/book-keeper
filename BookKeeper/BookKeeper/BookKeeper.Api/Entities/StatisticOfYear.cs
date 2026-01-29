namespace BookKeeper.Api.Entities;

public class StatisticOfYear
{
    private StatisticOfYear()
    {
    }

    public string Id { get; private set; }
    public int Year { get; private set; }
    public decimal TotalExpendAmount { get; private set; }
    public decimal TotalIncomeAmount { get; private set; }
    public decimal SumAmount { get; private set; }
    public string UserId { get; private set; }

    public static StatisticOfYear Create(
        int year,
        decimal totalExpendAmount,
        decimal totalIncomeAmount,
        string userId) =>
        new()
        {
            Id = $"soy_{Ulid.NewUlid()}",
            Year = year,
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
