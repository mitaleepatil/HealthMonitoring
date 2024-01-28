using System.ComponentModel.DataAnnotations;

namespace HealthMonitoring.Models;

public class MeasurementsRequest
{
    public MeasurementsRequest(List<Measurement> measurements)
    {
        Measurements = measurements;
    }
    [Required] public List<Measurement> Measurements { get; set; }
}