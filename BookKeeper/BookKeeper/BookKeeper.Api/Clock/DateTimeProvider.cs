namespace BookKeeper.Api.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo TaipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
    
    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTime TaipeiNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
    
    public DateOnly TaipeiToday => DateOnly.FromDateTime(TaipeiNow);
}
