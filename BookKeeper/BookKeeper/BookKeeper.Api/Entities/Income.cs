namespace BookKeeper.Api.Entities;

public sealed class Income
{
    private Income()
    {
    }

    public string Id { get; private set; }
    public string IncomeName { get; private set; }
    public decimal Amount { get; private set; }
    public string LabelId { get; private set; }
    public Label Label { get; private set; }
    public DateOnly IncomeDateOnUtc { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }

    public static Income Create(
        string incomeName,
        decimal amount,
        DateOnly incomeDateOnUtc,
        Label label) =>
        new()
        {
            Id = $"i_{Ulid.NewUlid()}",
            IncomeName = incomeName,
            Amount = amount,
            IncomeDateOnUtc = incomeDateOnUtc,
            Label = label,
            LabelId = label.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

    public void Update(
        string incomeName,
        decimal amount,
        DateOnly incomeDateOnUtc,
        Label label)
    {
        IncomeName = incomeName;
        Amount = amount;
        IncomeDateOnUtc = incomeDateOnUtc;
        Label = label;
        LabelId = label.Id;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
