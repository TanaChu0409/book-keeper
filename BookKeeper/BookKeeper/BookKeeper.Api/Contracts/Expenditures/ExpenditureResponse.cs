namespace BookKeeper.Api.Contracts.Expenditures;

public class ExpenditureResponse
{
    public string Id { get; set; }
    public string PaymentName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDateOnLocal { get; set; }
    public ExpenditureLabelResponse Label { get; set; }

    public class ExpenditureLabelResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
