using System.Collections.Immutable;

namespace HealthMonitoring.Models;

public class MeasurementTypeConfigReader
{
    private static MeasurementTypeConfigReader? _instance;

    public static MeasurementTypeConfigReader GetInstance(IConfiguration configuration)
    {
        return _instance ??= new MeasurementTypeConfigReader(configuration);
    }

    public ImmutableDictionary<MeasurementType, MeasurementTypeConfig> MeasurementTypes { get; }
    public ImmutableDictionary<Measurement, int?> MeasurementToScoreMapping { get; }
    
    private MeasurementTypeConfigReader(IConfiguration configuration)
    {
        var factory = LoggerFactory.Create(builder => {
            builder.AddConsole();
        });
        var logger = factory.CreateLogger<MeasurementTypeConfigReader>();
        logger.LogInformation("Loading measurement type configuration");
        MeasurementTypes = LoadMeasurementTypes(configuration);
        MeasurementToScoreMapping = LoadMeasurementToScoreMapping(configuration);
    }
    
    private static ImmutableDictionary<MeasurementType, MeasurementTypeConfig> LoadMeasurementTypes(IConfiguration configuration)
    {
        // Assumption: Config is validated
        var measurementTypes = configuration.GetSection("MeasurementTypes").Get<List<MeasurementTypeConfig>>();
        if (measurementTypes == null)
            throw new Exception("Missing Configuration");
        return measurementTypes.ToImmutableDictionary(x => x.Type);
    }
    
    private static ImmutableDictionary<Measurement, int?> LoadMeasurementToScoreMapping(IConfiguration configuration)
    {
        var mappings = new Dictionary<Measurement, int?>();
        var measurementTypes = LoadMeasurementTypes(configuration);
        foreach (var measurementTypeConfig in measurementTypes.Values)
        {
            foreach (var range in measurementTypeConfig.Ranges)
            {
                for (var value = range.LowRange + 1; value <= range.HighRange; ++value)
                {
                    mappings[new Measurement(measurementTypeConfig.Type, value)] = range.Score;
                }
            }
        }
        return mappings.ToImmutableDictionary();
    }
}