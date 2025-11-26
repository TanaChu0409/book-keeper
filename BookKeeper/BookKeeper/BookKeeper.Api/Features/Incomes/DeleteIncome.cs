using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Incomes;

public static class DeleteIncome
{
    public class Command : IRequest<Result>
    {
        public string IncomeId { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IncomeId).NotEmpty();
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
                        "DeleteIncome.Validation",
                        validationResult.ToString()));
            }

            Income? income = await dbContext.Incomes.FirstOrDefaultAsync(
                x => x.Id == request.IncomeId,
                cancellationToken);

            if (income is null)
            {
                return Result.Failure(
                    new Error(
                        "DeleteIncome.IncomeNotFound",
                        $"Income with ID '{request.IncomeId}' was not found"));
            }

            dbContext.Incomes.Remove(income);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteIncomeEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/incomes/{id}", async (string id, ISender sender) =>
        {
            Result result = await sender.Send(
                new DeleteIncome.Command
                {
                    IncomeId = id
                });

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Incomes);
    }
}
