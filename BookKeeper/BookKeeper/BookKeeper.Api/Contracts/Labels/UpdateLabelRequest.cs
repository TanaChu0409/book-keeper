namespace BookKeeper.Api.Contracts.Labels;

public sealed record UpdateLabelRequest(
    string Name,
    bool IsIncome);
