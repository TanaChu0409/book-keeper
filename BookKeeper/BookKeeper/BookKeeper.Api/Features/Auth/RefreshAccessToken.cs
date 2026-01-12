using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Clock;
using BookKeeper.Api.Contracts.Auth;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Services;
using BookKeeper.Api.Settings;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookKeeper.Api.Features.Auth;

public static class RefreshAccessToken
{
    public class Command : IRequest<Result<AccessTokenDto>>
    {
        public string RefreshToken { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }

    internal sealed class Handler(
        ApplicationIdentityDbContext identityDbContext,
        UserManager<IdentityUser> userManager,
        TokenProvider tokenProvider,
        IValidator<Command> validator,
        IOptions<JwtAuthOptions> jwtOptions,
        IDateTimeProvider dateTimeProvider)
        : IRequestHandler<Command, Result<AccessTokenDto>>
    {
        private readonly JwtAuthOptions _jwtOptions = jwtOptions.Value;

        public async Task<Result<AccessTokenDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Refresh.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            RefreshToken? storedToken = await identityDbContext.RefreshTokens
                .AsTracking()
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (storedToken is null)
            {
                return InvalidRefreshToken();
            }

            if (storedToken.ExpiresAtUtc <= dateTimeProvider.UtcNow)
            {
                identityDbContext.RefreshTokens.Remove(storedToken);
                await identityDbContext.SaveChangesAsync(cancellationToken);

                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Refresh.TokenExpired",
                        "The refresh token has expired.",
                        ErrorType.Problem));
            }

            IdentityUser? identityUser = await userManager.FindByIdAsync(storedToken.UserId);
            if (identityUser is null)
            {
                identityDbContext.RefreshTokens.Remove(storedToken);
                await identityDbContext.SaveChangesAsync(cancellationToken);

                return InvalidRefreshToken();
            }

            IList<string> roles = await userManager.GetRolesAsync(identityUser);

            AccessTokenDto tokens = tokenProvider.Create(
                new TokenRequest(
                    identityUser.Id,
                    identityUser.Email ?? string.Empty,
                    roles));

            await RotateRefreshTokenAsync(identityUser.Id, tokens.RefreshToken, cancellationToken);

            return tokens;
        }

        private async Task RotateRefreshTokenAsync(string identityUserId, string refreshToken, CancellationToken cancellationToken)
        {
            await identityDbContext.RefreshTokens
                .Where(rt => rt.UserId == identityUserId)
                .ExecuteDeleteAsync(cancellationToken);

            var token = new RefreshToken
            {
                Id = $"rt_{Ulid.NewUlid()}",
                UserId = identityUserId,
                Token = refreshToken,
                ExpiresAtUtc = dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            await identityDbContext.RefreshTokens.AddAsync(token, cancellationToken);
            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        private static Result<AccessTokenDto> InvalidRefreshToken() =>
            Result.Failure<AccessTokenDto>(
                new Error(
                    "Refresh.InvalidToken",
                    "The refresh token is invalid.",
                    ErrorType.Problem));
    }
}

public sealed class RefreshAccessTokenEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh", async (RefreshTokenRequest request, ISender sender) =>
        {
            Result<AccessTokenDto> result = await sender.Send(
                new RefreshAccessToken.Command
                {
                    RefreshToken = request.RefreshToken
                });

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .WithTags(Tags.Auth);
    }
}
