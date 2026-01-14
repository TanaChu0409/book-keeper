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
    public string LabelId { get; private set; }
    public Label Label { get; private set; }
    public string UserId { get; private set; }
    
    public static Expenditure Create(
        string paymentName,
        decimal amount,
        DateOnly paymentDateOnUtc,
        Label label,
        string userId) =>
        new()
        {
            Id = $"e_{Ulid.NewUlid()}",
            PaymentName = paymentName,
            Amount = amount,
            PaymentDateOnUtc = paymentDateOnUtc,
            Label = label,
            LabelId = label.Id,
            UserId = userId,
            CreatedOnUtc = DateTime.UtcNow
        };

    public void Update(
        string paymentName,
        decimal amount,
        DateOnly paymentDateOnUtc,
        Label label)
    {
        PaymentName = paymentName;
        Amount = amount;
        PaymentDateOnUtc = paymentDateOnUtc;
        Label = label;
        LabelId = label.Id;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
