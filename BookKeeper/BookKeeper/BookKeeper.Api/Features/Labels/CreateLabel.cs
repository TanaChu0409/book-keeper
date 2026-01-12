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

namespace BookKeeper.Api.Features.Labels;

public static class CreateLabel
{
    public class Command : IRequest<Result<string>>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
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
                        "CreateLabel.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(
                    new Error(
                        "CreateLabel.Validation",
                        validationResult.ToString(),
                        ErrorType.Validation));
            }

            var label = Label.Create(request.Name, request.IsIncome, userId);

            await dbContext.Labels.AddAsync(label, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return label.Id;
        }
    }
}

public class CreateLabelEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/labels", async (CreateLabelRequest request,ISender sender) =>
        {
            Result<string> result = await sender.Send(
                new CreateLabel.Command
                {
                    Name = request.Name,
                    IsIncome = request.IsIncome,
                });

            return result.Match(Results.Ok, Endpoints.ApiResults.Problem);
        })
        .WithTags(Tags.Labels);
    }
}
