namespace BookKeeper.Api.Contracts.Incomes;

public sealed record CreateIncomeRequest(
    string IncomeName,
    decimal Amount,
    DateOnly IncomeDateOnUtc,
    string LabelId);
