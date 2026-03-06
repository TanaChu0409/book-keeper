using BookKeeper.Api.Contracts.Labels;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Labels;

public static class GetLabel
{
    public class Query : IRequest<Result<LabelResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler(ApplicationDbContext dbContext, UserContext userContext)
        : IRequestHandler<Query, Result<LabelResponse>>
    {
        public async Task<Result<LabelResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<LabelResponse>(
                    new Error(
                        "GetLabel.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            LabelResponse? labelResponse = await dbContext
                .Labels
                .AsNoTracking()
                .Where(l => l.Id == request.Id &&
                            l.UserId == userId)
                .Select(l => new LabelResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    IsIncome = l.IsIncome
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (labelResponse is null)
            {
                return Result.Failure<LabelResponse>(
                    new Error(
                        "GetLabel.NotFound",
                        "The label with the specified ID was not found",
                        ErrorType.NotFound));
            }

            return labelResponse;
        }
    }
}

public class GetLabelEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/labels/{id}", async (string id, ISender sender) =>
        {
            Result<LabelResponse> result = await sender.Send(
                new GetLabel.Query
                {
                    Id = id
                });

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Labels);
    }
}
