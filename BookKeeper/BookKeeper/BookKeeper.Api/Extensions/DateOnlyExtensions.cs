namespace BookKeeper.Api.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly ToLocalDate(this DateOnly utcDate)
    {
        var utcDateTime = utcDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        
        var taipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        DateTime taipeiDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, taipeiTimeZone);
        var taipeiDateOnly = DateOnly.FromDateTime(taipeiDateTime);

        return taipeiDateOnly;
    }
}
