namespace BookKeeper.Api.Contracts.Auth;

public sealed record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
