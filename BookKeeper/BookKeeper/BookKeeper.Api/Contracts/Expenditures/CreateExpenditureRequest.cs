namespace BookKeeper.Api.Contracts.Expenditures;

public sealed record CreateExpenditureRequest(
    string PaymentName,
    decimal Amount,
    DateOnly PaymentDateOnLocal,
    string LabelId);
