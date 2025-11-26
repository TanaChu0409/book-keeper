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

public static class UpdateIncome
{
    public class Command : IRequest<Result>
    {
        public string IncomeId { get; set; } = string.Empty;
        public string IncomeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly IncomeDateOnUtc { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IncomeId)
                .NotEmpty();

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
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure(
                    new Error(
                        "UpdateIncome.Validation",
                        validationResult.ToString()));
            }

            Income? income = await dbContext.Incomes.FirstOrDefaultAsync(
                x => x.Id == request.IncomeId,
                cancellationToken);

            if (income is null)
            {
                return Result.Failure(
                    new Error(
                        "UpdateIncome.IncomeNotFound",
                        $"Income with ID '{request.IncomeId}' was not found."));
            }

            Label? label = await dbContext.Labels.FirstOrDefaultAsync(
                x =>
                    x.Id == request.LabelId &&
                    !x.IsDeleted,
                cancellationToken);

            if (label is null)
            {
                return Result.Failure<string>(
                    new Error(
                        "UpdateExpenditure.LabelNotFound",
                        $"Label with ID '{request.LabelId}' was not found."));
            }

            income.Update(
                request.IncomeName,
                request.Amount,
                request.IncomeDateOnUtc,
                label);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class UpdateIncomeEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("api/incomes/{id}", async (
            string id,
            UpdateIncomeRequest request,
            ISender sender) =>
        {
            Result result = await sender.Send(
                new UpdateIncome.Command
                {
                    IncomeId = id,
                    IncomeName = request.IncomeName,
                    Amount = request.Amount,
                    IncomeDateOnUtc = request.IncomeDateOnUtc,
                    LabelId = request.LabelId
                });

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Incomes);
    }
}
