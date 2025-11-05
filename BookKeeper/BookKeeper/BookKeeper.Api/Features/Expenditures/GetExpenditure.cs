using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Expenditures;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Expenditures;

public static class GetExpenditure
{
    public class Query : IRequest<Result<ExpenditureResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Query, Result<ExpenditureResponse>>
    {
        public async Task<Result<ExpenditureResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            ExpenditureResponse? expenditureResponse = await dbContext
                .Expenditures
                .AsNoTracking()
                .Where(expenditure => expenditure.Id == request.Id)
                .Include(x => x.Label)
                .Select(expenditure => new ExpenditureResponse
                {
                    Id = expenditure.Id,
                    PaymentName = expenditure.PaymentName,
                    Amount = expenditure.Amount,
                    PaymentDateOnLocal = expenditure.PaymentDateOnUtc,
                    Label = new ExpenditureResponse.ExpenditureLabelResponse
                    {
                        Id = expenditure.LabelId,
                        Name = expenditure.Label.Name
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (expenditureResponse is null)
            {
                return Result.Failure<ExpenditureResponse>(
                    new Error(
                        "GetExpenditure.Null",
                        "The expenditure with the specified ID was not found"));
            }

            return expenditureResponse;
        }
    }
}

public class GetExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenditure/{id}", async (string id, ISender sender) =>
        {
            Result<ExpenditureResponse> result = await sender.Send(
                new GetExpenditure.Query
                {
                    Id = id
                });

            return result.Match(
                onSuccess: (data) => Results.Ok(data),
                onFailure: (error) => Results.BadRequest(error));
        });
    }
}
