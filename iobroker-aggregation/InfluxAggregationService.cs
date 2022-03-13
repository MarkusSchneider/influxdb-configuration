using InfluxDB.Client;
using InfluxDB.Client.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IOBroker.Aggregation;

public sealed class InfluxOptions
{
    public const string SectionName = "Influx";
    public string Host { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
}

public sealed class MeasurementValue
{
    [Column(IsMeasurement = true)]
    public double Value { get; set; }

    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }
}

public sealed class InfluxAggregationService : BackgroundService
{
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
                    .Range(Timerange.Minute)
                    .Last()
                    .ToDebugString(query => _logger.LogDebug("Flux Query: {query}", query))
                    .QueryValueAsync();
                vals.ToList().ForEach(x => _logger.LogDebug($"{x.Timestamp} {x.Value}"));

                client.Dispose();
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return base.StopAsync(cancellationToken);
    }
    public enum Timerange
    {
        Minute,
        Hour,
        Day,
        Month,
        Year,
    }

    public sealed class FluxFluent
    {
        private string _query = string.Empty;
        private InfluxDBClient _client = null;

        public static FluxFluent From(InfluxDBClient client, string database, string retentionPolicy)
        {
            var instance = new FluxFluent();
            instance._client = client;
            instance._query = $"from(bucket: \"{database}/{retentionPolicy}\")";
            return instance;
        }

        public FluxFluent Range(Timerange timerange)
        {
            var now = DateTime.UtcNow;
            DateTime start = now;
            DateTime stop = now;

            switch (timerange)
            {
                case Timerange.Minute:
                    stop = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(1).AddSeconds(1);
                    start = stop.AddMinutes(-1);
                    break;
            }
            //https://docs.influxdata.com/flux/v0.x/stdlib/universe/range/
            Pipe($"range(start: {start.ToString("yyyy-MM-ddTHH:mm:ssZ")}, stop: {stop.ToString("yyyy-MM-ddTHH:mm:ssZ")})");

            return this;
        }

        public FluxFluent Last()
        {
            Pipe($"last()");

            return this;
        }

        public Task<IEnumerable<MeasurementValue>> QueryValueAsync()
        {
            Pipe("filter(fn: (r) => r._field ==\"value\")");
            return QueryAsync();
        }

        public FluxFluent ToDebugString(Action<string> fn)
        {
            fn(_query);
            return this;
        }

        private async Task<IEnumerable<MeasurementValue>> QueryAsync()
        {
            var queryApi = _client.GetQueryApi();
            var values = await queryApi.QueryAsync(_query);
            var result = new List<MeasurementValue>();

            foreach (var table in values)
            {
                foreach (var record in table.Records)
                {
                    result.Add(new MeasurementValue
                    {
                        Value = Convert.ToDouble(record.GetValue()),
                        Timestamp = record.GetTime().Value.ToDateTimeUtc()
                    });
                }
            }

            return result;
        }

        private void Pipe(string query)
        {
            _query += $" |> {query}";
        }
    }
}