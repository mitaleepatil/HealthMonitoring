using System.ComponentModel.DataAnnotations;

namespace HealthMonitoring.Models;

public class Measurement
{
    public Measurement(MeasurementType type, int value)
    {
        Type = type;
        Value = value;
    }

    [Required] public MeasurementType Type { get; }

    [Required] public int Value { get; }
    
    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Value.GetHashCode();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Measurement measurement)
        {
            return false;
        }
        return Type == measurement.Type && Value == measurement.Value;
    }
}