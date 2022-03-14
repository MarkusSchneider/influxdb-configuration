namespace IOBroker.Aggregation;

public sealed class TimeRange
{
    public TimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}
