namespace BookKeeper.Api.Contracts.Labels;

public sealed record CreateLabelRequest(
    string Name,
    bool IsIncome);
