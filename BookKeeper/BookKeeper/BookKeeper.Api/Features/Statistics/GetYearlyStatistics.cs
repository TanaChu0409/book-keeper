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

public static class GetYearlyStatistics
{
    public class Query : IRequest<Result<PaginationResult<YearlyStatisticResponse>>>
    {
        public int? Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            When(x => x.Year.HasValue, () =>
            {
                RuleFor(x => x.Year!.Value)
                    .InclusiveBetween(1900, 2100)
                    .WithMessage("Year must be between 1900 and 2100.");
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
        Validator validator)
        : IRequestHandler<Query, Result<PaginationResult<YearlyStatisticResponse>>>
    {
        public async Task<Result<PaginationResult<YearlyStatisticResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaginationResult<YearlyStatisticResponse>>(
                    new Error(
                        "GetYearlyStatistics.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<PaginationResult<YearlyStatisticResponse>>(
                    new Error(
                        "GetYearlyStatistics.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            IQueryable<StatisticOfYear> query = dbContext.StatisticsOfYears
                .AsNoTracking()
                .Where(s => s.UserId == userId);

            if (request.Year.HasValue)
            {
                query = query.Where(s => s.Year == request.Year.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            List<YearlyStatisticResponse> items = await query
                .OrderByDescending(s => s.Year)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new YearlyStatisticResponse
                {
                    Year = s.Year,
                    TotalExpendAmount = s.TotalExpendAmount,
                    TotalIncomeAmount = s.TotalIncomeAmount,
                    SumAmount = s.SumAmount
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<YearlyStatisticResponse>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

public class GetYearlyStatisticsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/statistics/yearly", async (int? year, int? page, int? pageSize, ISender sender) =>
        {
            Result<PaginationResult<YearlyStatisticResponse>> result = await sender.Send(
                new GetYearlyStatistics.Query
                {
                    Year = year,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Statistics)
        .RequireAuthorization();
    }
}
