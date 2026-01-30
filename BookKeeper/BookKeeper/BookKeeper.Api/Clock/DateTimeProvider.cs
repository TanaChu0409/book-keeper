namespace BookKeeper.Api.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo TaipeiTimeZone = CreateTaipeiTimeZone();

    public DateTime TaipeiNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
    public DateOnly TaipeiToday => DateOnly.FromDateTime(TaipeiNow);
    public DateTime UtcNow => DateTime.UtcNow;
    private static TimeZoneInfo CreateTaipeiTimeZone()
    {
        // Try Windows time zone ID (Windows systems)
        TimeZoneInfo? timeZone = TryGetTimeZone("Taipei Standard Time");
        
        if (timeZone is not null)
        {
            return timeZone;
        }

        // Try IANA time zone ID (Linux/macOS/Docker)
        timeZone = TryGetTimeZone("Asia/Taipei");
        if (timeZone is not null)
        {
            return timeZone;
        }

        // Fallback to UTC if neither time zone ID is available
        return TimeZoneInfo.Utc;
    }

    private static TimeZoneInfo? TryGetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) 
            when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Time zone not found on this system, return null to try alternative
            return null;
        }
    }
}
