namespace BookKeeper.Api.Contracts.Incomes;

public sealed record UpdateIncomeRequest(
    string IncomeName,
    decimal Amount,
    DateOnly IncomeDateOnUtc,
    string LabelId);
