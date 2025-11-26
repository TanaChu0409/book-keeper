using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Expenditures;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Expenditures;

public static class UpdateExpenditure
{
    public class Command: IRequest<Result>
    {
        public string ExpenditureId { get; set; } = string.Empty;
        public string PaymentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly PaymentDateOnUtc { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ExpenditureId)
                .NotEmpty();

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
        IValidator<Command> validator)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure(
                    new Error(
                        "UpdateExpenditure.Validation",
                        validationResult.ToString()));
            }

            Expenditure? expenditure = await dbContext.Expenditures.FirstOrDefaultAsync(
                x =>
                    x.Id == request.ExpenditureId,
                cancellationToken);

            if (expenditure is null)
            {
                return Result.Failure(
                    new Error(
                    "UpdateExpenditure.ExpeditureNotFound",
                    $"Expenditure with ID '{request.ExpenditureId}' was not found"));
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

            expenditure.Update(
                request.PaymentName,
                request.Amount,
                request.PaymentDateOnUtc,
                label);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class UpdateExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("api/expenditures/{id}", async (
            string id,
            UpdateExpenditureRequest request,
            ISender sender) =>
        {
            Result result = await sender.Send(
                new UpdateExpenditure.Command
                {
                    ExpenditureId = id,
                    PaymentName = request.PaymentName,
                    Amount = request.Amount,
                    PaymentDateOnUtc = request.PaymentDateOnUtc,
                    LabelId = request.LabelId
                });

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures);
    }
}
