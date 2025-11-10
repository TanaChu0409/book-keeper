using System.Dynamic;
using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Common;
using BookKeeper.Api.Contracts.Expenditures;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Expenditures;

public static class GetExpenditures
{
    public class Query : IRequest<Result<PaginationResult<ExpenditureResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<PaginationResult<ExpenditureResponse>>>
    {
        public async Task<Result<PaginationResult<ExpenditureResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            List<ExpenditureResponse> expenditureQuery = await dbContext
                .Expenditures
                .Include(e => e.Label)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .OrderByDescending(e => e.CreatedOnUtc)
                .Select(e => new ExpenditureResponse
                {
                    Id = e.Id,
                    PaymentName = e.PaymentName,
                    Amount = e.Amount,
                    PaymentDateOnLocal = e.PaymentDateOnUtc,
                    Label = new ExpenditureResponse.ExpenditureLabelResponse
                    {
                        Id = e.LabelId,
                        Name = e.Label.Name
                    }
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<ExpenditureResponse>
            {
                Items = expenditureQuery,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = expenditureQuery.Count
            };
        }
    }
}

public class GetExpendituresEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenditures", async (int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<ExpenditureResponse>> result = await sender.Send(
                new GetExpenditures.Query
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });
            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures);
    }
}
