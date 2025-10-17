namespace BookKeeper.Api.Entities;

public sealed class Label
{
    private Label()
    {
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsIncome { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }

    public static Label Create(
        string name,
        bool isIncome) =>
        new()
        {
            Id = $"l_{Ulid.NewUlid()}",
            Name = name,
            IsIncome = isIncome,
            IsDeleted = false,
            CreatedOnUtc = DateTime.UtcNow
        };

    public void Update(
        string name,
        bool isIncome)
    {
        Name = name;
        IsIncome = isIncome;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    public void Deleted()
    {
        IsDeleted = true;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
