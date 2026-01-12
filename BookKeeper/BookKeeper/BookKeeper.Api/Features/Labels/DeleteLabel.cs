using BookKeeper.Api.ApiResults;
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Services;
using BookKeeper.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.Labels;

public static class DeleteLabel
{
    public class Command : IRequest<Result>
    {
        public string Id { get; set; } = string.Empty;
    }

    internal sealed class Handler(ApplicationDbContext dbContext, UserContext userContext)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            string? userId = await userContext.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result.Failure(
                    new Error(
                        "DeleteLabel.Unauthorized",
                        "User is not authenticated.",
                        ErrorType.Problem));
            }

            Label? label = await dbContext.Labels.FirstOrDefaultAsync(
                l => l.Id == request.Id &&
                     l.UserId == userId,
                cancellationToken);
            if (label is null)
            {
                return Result.Failure(
                    new Error(
                        "Label.NotFound",
                        $"Label with id '{request.Id}' was not found.",
                        ErrorType.NotFound));
            }

            label.Deleted();

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public sealed class DeleteLabelEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/labels/{id}", async (string id, ISender sender) =>
        {
            Result result = await sender.Send(
                new DeleteLabel.Command
                {
                    Id = id
                });

            return result.Match(Results.NoContent, Endpoints.ApiResults.Problem);
        })
        .WithTags(Tags.Labels);
    }
}
