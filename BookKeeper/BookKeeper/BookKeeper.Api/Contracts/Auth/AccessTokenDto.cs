namespace BookKeeper.Api.Contracts.Auth;

public sealed record AccessTokenDto(string AccessToken, string RefreshToken);
