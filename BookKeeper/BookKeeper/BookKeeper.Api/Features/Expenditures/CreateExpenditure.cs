using BookKeeper.Api.Contracts.Expenditures;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Extensions;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Expenditures;

public static class CreateExpenditure
{
    public class Command : IRequest<Result<string>>
    {
        public string PaymentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly PaymentDateOnUtc { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PaymentName)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.PaymentDateOnUtc)
                .GreaterThan(DateOnly.MinValue);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.LabelId)
                .NotEmpty()
                .MaximumLength(500);
        }
    }

    internal sealed class Handler(
        ApplicationDbContext dbContext,
        IValidator<Command> validator,
        UserContext userContext) 
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateExpenditure.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateExpenditure.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            Label? label = await dbContext.Labels.FirstOrDefaultAsync(
                x => x.Id == request.LabelId &&
                     !x.IsDeleted &&
                     x.UserId == userId,
                cancellationToken);

            if (label is null)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateExpenditure.LabelNotFound",
                        $"Label with ID '{request.LabelId}' was not found.",
                        ErrorType.NotFound));
            }

            var expenditure = Expenditure.Create(
                request.PaymentName,
                request.Amount,
                request.PaymentDateOnUtc,
                label,
                userId);

            await dbContext.Expenditures.AddAsync(expenditure, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return expenditure.Id;
        }
    }
}

public class CreateExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/expenditures", async (CreateExpenditureRequest request, ISender sender) =>
        {
            Result<string> result = await sender.Send(
                new CreateExpenditure.Command
                {
                    PaymentName = request.PaymentName,
                    Amount = request.Amount,
                    PaymentDateOnUtc = request.PaymentDateOnUtc,
                    LabelId = request.LabelId
                });

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Expenditures);
    }
}
