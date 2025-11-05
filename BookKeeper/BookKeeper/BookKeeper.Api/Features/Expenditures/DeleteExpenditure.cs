using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Expenditures;

public static class DeleteExpenditure
{
    public class Command : IRequest<Result>
    {
        public string ExpenditureId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ExpenditureId).NotEmpty();
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
                return Result.Failure<string>(
                    new Error(
                        "DeleteExpenditure.Validation",
                        validationResult.ToString()));
            }

            Expenditure? expenditure = await dbContext.Expenditures.FirstOrDefaultAsync(
                x => x.Id == request.ExpenditureId, cancellationToken);

            if (expenditure is null)
            {
                return Result.Failure(
                    new Error(
                    "DeleteExpenditure.ExpeditureNotFound",
                    $"Expenditure with ID '{request.ExpenditureId}' was not found"));
            }

            dbContext.Expenditures.Remove(expenditure);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/expenditures/{id}", async (string id, ISender sender) =>
        {
            Result result = await sender.Send(
                new DeleteExpenditure.Command
                {
                    ExpenditureId = id
                });

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: (error) => Results.BadRequest(error));
        });
    }
}
