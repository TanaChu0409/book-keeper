namespace BookKeeper.Api.Contracts.Expenditures;

public sealed record UpdateExpenditureRequest(
    string PaymentName,
    decimal Amount,
    DateOnly PaymentDateOnUtc,
    string LabelId);
