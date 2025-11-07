using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Labels;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
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

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<LabelResponse>>
    {
        public async Task<Result<LabelResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            LabelResponse? labelResponse = await dbContext
                .Labels
                .AsNoTracking()
                .Where(l => l.Id == request.Id)
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
                        "GetLabel.Null",
                        "The label with the specified ID was not found"));
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

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Labels);
    }
}
