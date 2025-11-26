using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Incomes;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Incomes;

public static class CreateIncome
{
    public class Command : IRequest<Result<string>>
    {
        public string IncomeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly IncomeDateOnUtc { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IncomeName)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.IncomeDateOnUtc)
                .GreaterThan(DateOnly.MinValue);

            RuleFor(x => x.LabelId)
                .NotEmpty()
                .MaximumLength(500);
        }
    }

    internal sealed class Handler(
        ApplicationDbContext dbContext,
        IValidator<Command> validator)
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateIncome.Validation",
                        validationResult.ToString()));
            }

            Label? label = await dbContext.Labels.FirstOrDefaultAsync(
                x =>
                    x.Id == request.LabelId &&
                    x.IsIncome,
                cancellationToken);

            if (label is null)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateIncome.LabelNotFound",
                        $"Label with ID '{request.LabelId}' was not found."));
            }

            var income = Income.Create(
                request.IncomeName,
                request.Amount,
                request.IncomeDateOnUtc,
                label);

            await dbContext.Incomes.AddAsync(income, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return income.Id;
        }
    }
}

public class CreateIncomeEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/incomes", async (CreateIncomeRequest request, ISender sender) =>
        {
            Result<string> result = await sender.Send(
                new CreateIncome.Command
                {
                    IncomeName = request.IncomeName,
                    Amount = request.Amount,
                    IncomeDateOnUtc = request.IncomeDateOnUtc,
                    LabelId = request.LabelId
                });

            return result.Match(
                onSuccess: (value) => Results.Ok(value),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Incomes);
    }
}
