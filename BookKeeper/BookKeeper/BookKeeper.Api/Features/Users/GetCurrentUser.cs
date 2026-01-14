using BookKeeper.Api.Contracts.Users;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Users;

public static class GetCurrentUser
{
    public class Query : IRequest<Result<UserResponse>>
    {
    }

    internal sealed class Handler(ApplicationDbContext dbContext, UserContext userContext)
        : IRequestHandler<Query, Result<UserResponse>>
    {
        public async Task<Result<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<UserResponse>(
                    new Error(
                        "GetCurrentUser.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            UserResponse? user = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
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
                        "GetCurrentUser.NotFound",
                        "The current user profile was not found.",
                        ErrorType.NotFound));
            }

            return user;
        }
    }
}

public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", async (ISender sender) =>
        {
            Result<UserResponse> result = await sender.Send(new GetCurrentUser.Query());

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
