using InfluxDB.Client;

namespace IOBroker.Aggregation;

public sealed partial class InfluxAggregationService
{
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

        public FluxFluent Range(TimeRange timerange)
        {
            //https://docs.influxdata.com/flux/v0.x/stdlib/universe/range/
            Pipe($"range(start: {timerange.Start.ToString("yyyy-MM-ddTHH:mm:ssZ")}, stop: {timerange.End.ToString("yyyy-MM-ddTHH:mm:ssZ")})");

            return this;
        }

        public FluxFluent Last()
        {
            Pipe($"last()");

            return this;
        }

        public FluxFluent Measurement(string measurement)
        {
            Pipe($"filter(fn: (r) => r._measurement  == \"{measurement}\")");
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