using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Common;
using BookKeeper.Api.Contracts.Incomes;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Incomes;

public static class GetIncomes
{
    public class Query : IRequest<Result<PaginationResult<IncomeResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<PaginationResult<IncomeResponse>>>
    {
        public async Task<Result<PaginationResult<IncomeResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            List<IncomeResponse> incomeQuery = await dbContext
                .Incomes
                .Include(i => i.Label)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .OrderByDescending(e => e.CreatedOnUtc)
                .Select(i => new IncomeResponse
                {
                    Id = i.Id,
                    IncomeName = i.IncomeName,
                    Amount = i.Amount,
                    IncomeDateOnLocal = i.IncomeDateOnUtc.ToLocalDate(),
                    Label = new IncomeResponse.IncomeLabelResponse
                    {
                        Id = i.LabelId,
                        Name = i.Label.Name
                    }
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<IncomeResponse>
            {
                Items = incomeQuery,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = incomeQuery.Count
            };
        }
    }
}

public class GetIncomesEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/incomes", async (int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<IncomeResponse>> result = await sender.Send(
                new GetIncomes.Query
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Incomes);
    }
}
