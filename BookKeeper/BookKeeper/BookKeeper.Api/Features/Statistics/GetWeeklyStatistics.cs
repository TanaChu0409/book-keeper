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

public static class GetWeeklyStatistics
{
    public class Query : IRequest<Result<PaginationResult<WeeklyStatisticResponse>>>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? WeekOfMonth { get; set; }
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

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12)
                .WithMessage("Month must be between 1 and 12.");

            When(x => x.WeekOfMonth.HasValue, () =>
            {
                RuleFor(x => x.WeekOfMonth!.Value)
                    .InclusiveBetween(1, 5)
                    .WithMessage("WeekOfMonth must be between 1 and 5.");
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
        : IRequestHandler<Query, Result<PaginationResult<WeeklyStatisticResponse>>>
    {
        public async Task<Result<PaginationResult<WeeklyStatisticResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaginationResult<WeeklyStatisticResponse>>(
                    new Error(
                        "GetWeeklyStatistics.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<PaginationResult<WeeklyStatisticResponse>>(
                    new Error(
                        "GetWeeklyStatistics.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            IQueryable<StatisticOfWeek> query = dbContext.StatisticsOfWeeks
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.Year == request.Year && s.Month == request.Month);

            if (request.WeekOfMonth.HasValue)
            {
                query = query.Where(s => s.WeekOfMonth == request.WeekOfMonth.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            List<WeeklyStatisticResponse> items = await query
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .ThenByDescending(s => s.WeekOfMonth)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new WeeklyStatisticResponse
                {
                    Year = s.Year,
                    Month = s.Month,
                    WeekOfMonth = s.WeekOfMonth,
                    TotalExpendAmount = s.TotalExpendAmount,
                    TotalIncomeAmount = s.TotalIncomeAmount,
                    SumAmount = s.SumAmount
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<WeeklyStatisticResponse>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

public class GetWeeklyStatisticsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/statistics/weekly", async (
            int year,
            int month,
            int? weekOfMonth,
            int? page,
            int? pageSize,
            ISender sender) =>
        {
            Result<PaginationResult<WeeklyStatisticResponse>> result = await sender.Send(
                new GetWeeklyStatistics.Query
                {
                    Year = year,
                    Month = month,
                    WeekOfMonth = weekOfMonth,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Statistics)
        .RequireAuthorization();
    }
}