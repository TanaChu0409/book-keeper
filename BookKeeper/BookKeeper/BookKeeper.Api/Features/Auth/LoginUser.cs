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

public static class LoginUser
{
    public class Command : IRequest<Result<AccessTokenDto>>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }

    internal sealed class Handler(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext applicationDbContext,
        ApplicationIdentityDbContext identityDbContext,
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
                        "Login.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            IdentityUser? identityUser = await userManager.FindByEmailAsync(request.Email);
            if (identityUser is null)
            {
                return InvalidCredentials();
            }

            bool isPasswordValid = await userManager.CheckPasswordAsync(identityUser, request.Password);
            if (!isPasswordValid)
            {
                return InvalidCredentials();
            }

            User? user = await applicationDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id, cancellationToken);

            if (user is null)
            {
                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Login.UserProfileNotFound",
                        "The user profile was not found.",
                        ErrorType.NotFound));
            }

            IList<string> roles = await userManager.GetRolesAsync(identityUser);

            AccessTokenDto tokens = tokenProvider.Create(
                new TokenRequest(
                    identityUser.Id,
                    identityUser.Email ?? request.Email,
                    roles));

            await RotateRefreshTokenAsync(identityUser.Id, tokens.RefreshToken, cancellationToken);

            return tokens;
        }

        private Result<AccessTokenDto> InvalidCredentials() =>
            Result.Failure<AccessTokenDto>(
                new Error(
                    "Login.InvalidCredentials",
                    "Invalid email or password.",
                    ErrorType.Problem));

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
    }
}

public sealed class LoginUserEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (LoginUserRequest request, ISender sender) =>
        {
            Result<AccessTokenDto> result = await sender.Send(
                new LoginUser.Command
                {
                    Email = request.Email,
                    Password = request.Password
                });

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
