namespace BookKeeper.Api.Contracts.Expenditures;

public record CreateExpenditureRequest(
    string PaymentName,
    decimal Amount,
    DateOnly PaymentDateOnLocal,
    string LabelId);
