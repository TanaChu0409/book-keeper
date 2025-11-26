using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Incomes;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Incomes;

public static class GetIncome
{
    public class Query : IRequest<Result<IncomeResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<IncomeResponse>>
    {
        public async Task<Result<IncomeResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            IncomeResponse? incomeResponse = await dbContext
                .Incomes
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Include(i => i.Label)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (incomeResponse is null)
            {
                return Result.Failure<IncomeResponse>(
                    new Error(
                            "GetIncome.Null",
                            "The income with the specified ID was not found"));
            }

            return incomeResponse;
        }
    }
}

public class GetIncomeEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/incomes/{id}", async (string id, ISender sender) =>
        {
            Result<IncomeResponse> result = await sender.Send(
                new GetIncome.Query
                {
                    Id = id
                });

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Incomes);
    }
}
