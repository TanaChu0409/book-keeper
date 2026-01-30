namespace BookKeeper.Api.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    
    /// <summary>
    /// 取得台灣時區（UTC+8）的當前時間
    /// </summary>
    DateTime TaipeiNow { get; }
    
    /// <summary>
    /// 取得台灣時區（UTC+8）的當前日期
    /// </summary>
    DateOnly TaipeiToday { get; }
}
