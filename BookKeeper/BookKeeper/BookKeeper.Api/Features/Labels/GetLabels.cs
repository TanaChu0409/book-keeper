using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Common;
using BookKeeper.Api.Contracts.Labels;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Labels;

public static class GetLabels
{
    public class Query : IRequest<Result<PaginationResult<LabelResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<PaginationResult<LabelResponse>>>
    {
        public async Task<Result<PaginationResult<LabelResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            List<LabelResponse> labelQuery = await dbContext
                .Labels
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(label => new LabelResponse
                {
                    Id = label.Id,
                    Name = label.Name,
                    IsIncome = label.IsIncome
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<LabelResponse>
            {
                Items = labelQuery,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = labelQuery.Count
            };
        }
    }
}

public class GetLabelsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/labels", async (int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<LabelResponse>> result = await sender.Send(
                new GetLabels.Query
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Labels);
    }
}
