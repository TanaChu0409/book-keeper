namespace BookKeeper.Api.Entities;

public class Expenditure
{
    public string Id { get; private set; }
    public string PaymentName { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDateOnUtc { get; private set; }
    public DateOnly CreatedOnUtc { get; private set; }
    public DateOnly? UpdatedOnUtc { get; private set; }
}
