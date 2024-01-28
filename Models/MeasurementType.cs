using System.Text.Json.Serialization;

namespace HealthMonitoring.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeasurementType
{
    // ReSharper disable once InconsistentNaming
    TEMP = 1,
    // ReSharper disable once InconsistentNaming
    HR,
    // ReSharper disable once InconsistentNaming
    RR
}