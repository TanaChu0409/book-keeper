namespace BookKeeper.Api.Contracts.Auth;

public sealed record TokenRequest(string UserId, string Email, IEnumerable<string> Roles);
