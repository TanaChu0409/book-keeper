namespace BookKeeper.Api.Contracts.Incomes;

public sealed class IncomeResponse
{
    public string Id { get; set; }
    public string IncomeName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly IncomeDateOnLocal { get; set; }
    public IncomeLabelResponse Label { get; set; }

    public sealed class IncomeLabelResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
