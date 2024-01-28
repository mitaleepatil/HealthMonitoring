namespace HealthMonitoring.Models;

public class MeasurementTypeConfig
{
    public MeasurementType Type { get; set; }
    public List<Range> Ranges { get; set; }
}