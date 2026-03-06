using BookKeeper.Api.Contracts.Common;
using BookKeeper.Api.Contracts.Statistics;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Statistics;

public static class GetMonthlyStatistics
{
    public class Query : IRequest<Result<PaginationResult<MonthlyStatisticResponse>>>
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(1900, 2100)
                .WithMessage("Year must be between 1900 and 2100.");

            When(x => x.Month.HasValue, () =>
            {
                RuleFor(x => x.Month!.Value)
                    .InclusiveBetween(1, 12)
                    .WithMessage("Month must be between 1 and 12.");
            });

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");
        }
    }

    internal sealed class Handler(
        ApplicationDbContext dbContext,
        UserContext userContext,
        IValidator<Query> validator)
        : IRequestHandler<Query, Result<PaginationResult<MonthlyStatisticResponse>>>
    {
        public async Task<Result<PaginationResult<MonthlyStatisticResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaginationResult<MonthlyStatisticResponse>>(
                    new Error(
                        "GetMonthlyStatistics.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<PaginationResult<MonthlyStatisticResponse>>(
                    new Error(
                        "GetMonthlyStatistics.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            IQueryable<StatisticOfMonth> query = dbContext.StatisticsOfMonths
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.Year == request.Year);

            if (request.Month.HasValue)
            {
                query = query.Where(s => s.Month == request.Month.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            List<MonthlyStatisticResponse> items = await query
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new MonthlyStatisticResponse
                {
                    Year = s.Year,
                    Month = s.Month,
                    TotalExpendAmount = s.TotalExpendAmount,
                    TotalIncomeAmount = s.TotalIncomeAmount,
                    SumAmount = s.SumAmount
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<MonthlyStatisticResponse>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

public class GetMonthlyStatisticsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/statistics/monthly", async (int year, int? month, int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<MonthlyStatisticResponse>> result = await sender.Send(
                new GetMonthlyStatistics.Query
                {
                    Year = year,
                    Month = month,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Statistics)
        .RequireAuthorization();
    }
}