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

public static class GetDailyStatistics
{
    public class Query : IRequest<Result<PaginationResult<DailyStatisticResponse>>>
    {
        public DateOnly? Date { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x)
                .Must(x => !(x.Date.HasValue && (x.StartDate.HasValue || x.EndDate.HasValue)))
                .WithMessage("Cannot specify both Date and StartDate/EndDate.")
                .Must(x => !(x.StartDate.HasValue && !x.EndDate.HasValue))
                .WithMessage("EndDate is required when StartDate is specified.")
                .Must(x => !(x.EndDate.HasValue && !x.StartDate.HasValue))
                .WithMessage("StartDate is required when EndDate is specified.");

            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.StartDate!.Value <= x.EndDate!.Value)
                    .WithMessage("StartDate must be less than or equal to EndDate.");
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
        : IRequestHandler<Query, Result<PaginationResult<DailyStatisticResponse>>>
    {
        public async Task<Result<PaginationResult<DailyStatisticResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaginationResult<DailyStatisticResponse>>(
                    new Error(
                        "GetDailyStatistics.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<PaginationResult<DailyStatisticResponse>>(
                    new Error(
                        "GetDailyStatistics.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            IQueryable<StatisticOfDate> query = dbContext.StatisticsOfDates
                .AsNoTracking()
                .Where(s => s.UserId == userId);

            if (request.Date.HasValue)
            {
                query = query.Where(s => s.DateOnUtc == request.Date.Value);
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(s => s.DateOnUtc >= request.StartDate.Value &&
                                         s.DateOnUtc <= request.EndDate.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            List<DailyStatisticResponse> items = await query
                .OrderByDescending(s => s.DateOnUtc)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new DailyStatisticResponse
                {
                    Date = s.DateOnUtc,
                    TotalExpendAmount = s.TotalExpendAmount,
                    TotalIncomeAmount = s.TotalIncomeAmount,
                    SumAmount = s.SumAmount
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<DailyStatisticResponse>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

public class GetDailyStatisticsEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/statistics/daily", async (
            DateOnly? date,
            DateOnly? startDate,
            DateOnly? endDate,
            int? page,
            int? pageSize,
            ISender sender) =>
        {
            Result<PaginationResult<DailyStatisticResponse>> result = await sender.Send(
                new GetDailyStatistics.Query
                {
                    Date = date,
                    StartDate = startDate,
                    EndDate = endDate,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                });
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Statistics)
        .RequireAuthorization();
    }
}
