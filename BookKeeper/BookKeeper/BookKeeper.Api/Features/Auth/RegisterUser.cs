using BookKeeper.Api.Clock;
using BookKeeper.Api.Contracts.Auth;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Services;
using BookKeeper.Api.Settings;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookKeeper.Api.Features.Auth;

public static class RegisterUser
{
    public class Command : IRequest<Result<AccessTokenDto>>
    {
        public string Email { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");
        }
    }

    internal sealed class Handler(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext applicationDbContext,
        ApplicationIdentityDbContext identityDbContext,
        TokenProvider tokenProvider,
        IValidator<Command> validator,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtAuthOptions> jwtOptions,
        ILogger<Handler> logger)
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
                        "Register.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
            applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
            await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

            IdentityUser? existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Register.EmailInUse",
                        "The email is already registered.",
                        ErrorType.Conflict));
            }

            var identityUser = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            IdentityResult createIdentityResult = await userManager.CreateAsync(identityUser, request.Password);
            if (!createIdentityResult.Succeeded)
            {
                var extensions = new Dictionary<string, object?>
                {
                    {
                        "errors",
                        createIdentityResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };
                logger.LogError("{@Extensions}", extensions);

                string errors = string.Join(
                    "; ",
                    createIdentityResult.Errors.Select(e => e.Description));


                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Register.IdentityCreationFailed",
                        errors,
                        ErrorType.Validation));
            }

            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);
            if (!addToRoleResult.Succeeded)
            {
                var extensions = new Dictionary<string, object?>
                {
                    {
                        "errors",
                        addToRoleResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };
                logger.LogError("{@Extensions}", extensions);

                string errors = string.Join(
                    "; ",
                    addToRoleResult.Errors.Select(e => e.Description));

                return Result.Failure<AccessTokenDto>(
                    new Error(
                        "Register.RoleAssignmentFailed",
                        errors,
                        ErrorType.Problem));
            }

            var user = User.Create(
                request.Email,
                request.Name, 
                dateTimeProvider.UtcNow);

            user.SetIdentityId(identityUser.Id);

            await applicationDbContext.Users.AddAsync(user, cancellationToken);
            await applicationDbContext.SaveChangesAsync(cancellationToken);

            IList<string> roles = await userManager.GetRolesAsync(identityUser);

            AccessTokenDto tokens = tokenProvider.Create(
                new TokenRequest(
                    identityUser.Id, 
                    request.Email, 
                    roles));

            await RotateRefreshTokenAsync(
                identityUser.Id, 
                tokens.RefreshToken, 
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return tokens;
        }

        private async Task RotateRefreshTokenAsync(
            string identityUserId, 
            string refreshToken, 
            CancellationToken cancellationToken)
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

public sealed class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (RegisterUserRequest request, ISender sender) =>
        {
            Result<AccessTokenDto> result = await sender.Send(
                new RegisterUser.Command
                {
                    Email = request.Email,
                    Name = request.Name,
                    Password = request.Password,
                    ConfirmPassword = request.ConfirmPassword
                });

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
