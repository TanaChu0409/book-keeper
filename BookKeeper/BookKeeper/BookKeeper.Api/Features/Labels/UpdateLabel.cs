using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Contracts.Labels;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Labels;

public static class UpdateLabel
{
    public class Command : IRequest<Result>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(500);
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
                        "UpdateLabel.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure(
                    new Error(
                        "UpdateLabel.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            Label? label = await dbContext.Labels.FirstOrDefaultAsync(
                l => l.Id == request.Id &&
                     l.UserId == userId,
                cancellationToken);

            if (label is null)
            {
                return Result.Failure(
                    new Error(
                    "UpdateLabel.NotFound",
                    $"Label with ID '{request.Id}' was not found.",
                    ErrorType.NotFound));
            }

            label.Update(request.Name, request.IsIncome);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class UpdateLabelEnpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("api/labels/{id}", async (
            string id,
            UpdateLabelRequest request,
            ISender sender) =>
        {
            Result result = await sender.Send(
                new UpdateLabel.Command
                {
                    Id = id,
                    Name = request.Name,
                    IsIncome = request.IsIncome,
                });

            return result.Match(Results.NoContent, Endpoints.ApiResults.Problem);
        })
        .WithTags(Tags.Labels);
    }
}
