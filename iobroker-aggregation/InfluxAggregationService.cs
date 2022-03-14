using InfluxDB.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace IOBroker.Aggregation;

public sealed partial class InfluxAggregationService : BackgroundService
{
    private const string electricalPowerIdentifier = "smartmeter.0.1-0:1_8_0__255.value";
    private const string electricalConsumptionIdentifier = "smartmeter.0.1-0:16_7_0__255.value";
    private InfluxOptions _connectionOptions;
    private bool _running = true;
    private readonly ILogger<InfluxAggregationService> _logger;

    public InfluxAggregationService(ILogger<InfluxAggregationService> logger, IOptions<InfluxOptions> options)
    {
        _connectionOptions = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_running)
        {
            try
            {
                var timestamp = DateTime.UtcNow.AddMinutes(-1);
                var host = $"http://{_connectionOptions.Host}:8086";
                var database = "iot_short_term";
                var retentionPolicy = "retention_1h";
                var client = InfluxDBClientFactory.CreateV1(
                    host,
                    _connectionOptions.User,
                    _connectionOptions.Password.ToCharArray(),
                    database,
                    retentionPolicy
                );

                var vals = await FluxFluent.From(client, database, retentionPolicy)
                    .Range(CalculateTimeRange(TimeInterval.Minute))
                    .Measurement(electricalPowerIdentifier)
                    .Last()
                    .ToDebugString(query => _logger.LogDebug("Flux Query: {query}", query))
                    .QueryValueAsync();
                vals.Select(x => NormalizeTimestamp(x, TimeInterval.Minute))
                    .ToList()
                    .ForEach(x => _logger.LogDebug($"{x.Timestamp} {x.Value}"));

                client.Dispose();
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }

    private MeasurementValue NormalizeTimestamp(MeasurementValue value, TimeInterval timeInterval)
    {
        switch (timeInterval)
        {
            case TimeInterval.Minute:
                var ts = value.Timestamp.AddMinutes(1);
                value.Timestamp = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, 0, DateTimeKind.Utc);
                break;
        }

        return value;
    }

    private TimeRange CalculateTimeRange(TimeInterval timeInterval)
    {
        var now = DateTime.UtcNow;
        DateTime start = now;
        DateTime end = now;

        switch (timeInterval)
        {
            case TimeInterval.Minute:
                end = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(1).AddMilliseconds(1);
                start = end.AddMinutes(-1);
                break;
        }

        return new TimeRange(start, end);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return base.StopAsync(cancellationToken);
    }
}