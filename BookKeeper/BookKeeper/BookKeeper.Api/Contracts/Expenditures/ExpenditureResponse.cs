namespace BookKeeper.Api.Contracts.Expenditures;

public sealed class ExpenditureResponse
{
    public string Id { get; set; }
    public string PaymentName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDateOnLocal { get; set; }
    public ExpenditureLabelResponse Label { get; set; }

    public sealed class ExpenditureLabelResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
