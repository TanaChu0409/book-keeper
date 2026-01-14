using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Services;
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
        IValidator<Command> validator,
        UserContext userContext)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure(
                    new Error(
                        "DeleteExpenditure.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure(
                    new Error(
                        "DeleteExpenditure.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            Expenditure? expenditure = await dbContext.Expenditures.FirstOrDefaultAsync(
                x => x.Id == request.ExpenditureId && 
                     x.UserId == userId, 
                cancellationToken);

            if (expenditure is null)
            {
                return Result.Failure(
                    new Error(
                        "DeleteExpenditure.ExpeditureNotFound",
                        $"Expenditure with ID '{request.ExpenditureId}' was not found",
                        ErrorType.NotFound));
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

            return result.Match(Results.NoContent, Endpoints.ApiResults.Problem);
        })
        .WithTags(Tags.Expenditures);
    }
}
