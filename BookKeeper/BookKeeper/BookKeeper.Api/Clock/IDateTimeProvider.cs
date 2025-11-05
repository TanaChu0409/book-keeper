namespace BookKeeper.Api.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
