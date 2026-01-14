using BookKeeper.Api.Contracts.Users;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Users;

public static class GetUserById
{
    public class Query : IRequest<Result<UserResponse>>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }

    internal sealed class Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
        : IRequestHandler<Query, Result<UserResponse>>
    {
        public async Task<Result<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<UserResponse>(
                    new Error(
                        "GetUserById.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            UserResponse? user = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == request.Id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    CreatedAtUtc = u.CreatedAtUtc,
                    UpdatedAtUtc = u.UpdatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponse>(
                    new Error(
                        "GetUserById.NotFound",
                        "The user with the specified ID was not found.",
                        ErrorType.NotFound));
            }

            return user;
        }
    }
}

public sealed class GetUserByIdEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}", async (string id, ISender sender) =>
        {
            Result<UserResponse> result = await sender.Send(
                new GetUserById.Query
                {
                    Id = id
                });

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
        .WithTags(Tags.Users);
    }
}
