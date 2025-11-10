using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Common;
using BookKeeper.Api.Contracts.Labels;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Labels;

public static class GetExpenditureLabels
{
    public class Query : IRequest<PaginationResult<LabelResponse>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, PaginationResult<LabelResponse>>
    {
        public async Task<PaginationResult<LabelResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            List<LabelResponse> labelQuery = await dbContext
                 .Labels
                 .Where(l => !l.IsIncome)
                 .Skip((request.Page - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .OrderByDescending(l => l.CreatedOnUtc)
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

public class GetExpenditureLabelsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/labels/expenditures", async (int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<LabelResponse>> result = await sender.Send(
                new GetExpenditureLabels.Query
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (errors) => Results.BadRequest(errors));
        })
        .WithTags(Tags.Labels);
    }
}
