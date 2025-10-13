namespace BookKeeper.Api.Entities;

public sealed class Expenditure
{
    private Expenditure()
    {
    }

    public string Id { get; private set; }
    public string PaymentName { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDateOnUtc { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }

    public static Expenditure Create(
        string paymentName,
        decimal amount,
        DateOnly paymentDateOnUtc) =>
        new()
        {
            Id = $"e_{Ulid.NewUlid()}",
            PaymentName = paymentName,
            Amount = amount,
            PaymentDateOnUtc = paymentDateOnUtc,
            CreatedOnUtc = DateTime.UtcNow
        };
}
