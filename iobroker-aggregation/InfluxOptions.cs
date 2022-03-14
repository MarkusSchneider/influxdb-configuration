namespace IOBroker.Aggregation;

public sealed class InfluxOptions
{
    public const string SectionName = "Influx";
    public string Host { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
}
