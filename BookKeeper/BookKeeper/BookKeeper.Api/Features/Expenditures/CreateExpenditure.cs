using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Expenditures;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
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
        public DateOnly PaymentDateOnLocal { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PaymentName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.PaymentDateOnLocal)
                .GreaterThan(DateOnly.MinValue);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.LabelId)
                .NotEmpty()
                .MaximumLength(100);
        }
    }

    internal sealed class Handler(
        ApplicationDbContext dbContext,
        IValidator<Command> validator) 
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateExpenditure.Validation",
                        validationResult.ToString()));
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
                        "CreateExpenditure.LabelNotFound",
                        $"Label with ID '{request.LabelId}' was not found."));
            }

            var expenditure = Expenditure.Create(
                request.PaymentName,
                request.Amount,
                request.PaymentDateOnLocal,
                label);

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
                    PaymentDateOnLocal = request.PaymentDateOnLocal,
                    LabelId = request.LabelId
                });

            return result.Match(
                onSuccess: (value) => Results.Ok(value),
                onFailure: (error) => Results.BadRequest(error));
        });
    }
}
