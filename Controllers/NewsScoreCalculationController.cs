using System.Collections.Immutable;
using HealthMonitoring.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HealthMonitoring.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsScoreCalculationController : Controller
{
    private readonly IConfiguration _configuration;
    
    public NewsScoreCalculationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    [HttpGet(Name = "GetNewsScore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Results<BadRequest<string>, Ok<ScoreResult>> GetNewsScore(MeasurementsRequest measurementsRequest)
    {
       try
       {
           var (errors, score) = ProcessMeasurements(measurementsRequest.Measurements);
           if (errors.Any())
           {
               return TypedResults.BadRequest(string.Join(',', errors));
           }
           return TypedResults.Ok(new ScoreResult
           { 
               Score = score
           });
       } 
       catch (Exception e)
       { 
           return TypedResults.BadRequest(e.Message);
       }
    }

    private (List<string>,int) ProcessMeasurements(List<Measurement> measurements)
    {
        var measurementTypes = MeasurementTypeConfigReader.GetInstance(_configuration).MeasurementTypes;
        var newsScore = 0;
        var unprocessedTypes = measurementTypes.Keys.ToHashSet();
        var lstErrors = new List<string>();
        foreach (var measurement in measurements)
        {
            var measurementType = measurementTypes.GetValueOrDefault(measurement.Type);
            // Type Validation
            if (measurementType == null)
            {
                lstErrors.Add($"Invalid Type: {measurement.Type}");
                continue;
            }

            if (unprocessedTypes.Contains(measurement.Type))
            {
                unprocessedTypes.Remove(measurement.Type);
            }
            else
            {
                lstErrors.Add($"Duplicate Type: {measurement.Type}");
                continue;
            }

            //var score = CalculateScore(measurementType, measurement);
            //var score = CalculateScoreFast(measurement);
            var score = CalculateScoreVeryFast(measurement);
            if (score != null)
            {
                newsScore += score.Value;
            }
            else
            {
                lstErrors.Add($"Out of Range: {measurement.Value} for Type: {measurement.Type}");
            }
        }

        // Missing Type
        foreach (var type in unprocessedTypes)
        {
            lstErrors.Add($"Missing Type: {type}");
        }
        return (lstErrors, newsScore);
     }

    private static int? CalculateScore(MeasurementTypeConfig measurementTypeConfig, Measurement measurement)
    {
        foreach (var range in measurementTypeConfig.Ranges)
        {
            if (range.LowRange < measurement.Value && measurement.Value <= range.HighRange)
            {
                return range.Score;
            }
        }
        return null;
    }
    
    private static int? CalculateScoreFast(Measurement measurement)
    {
        return measurement.Type switch 
        {
            MeasurementType.TEMP when measurement.Value is > 31 and <= 35  => 3,
            MeasurementType.TEMP when measurement.Value is > 35 and <= 36  => 1,
            MeasurementType.TEMP when measurement.Value is > 36 and <= 38  => 0,
            MeasurementType.TEMP when measurement.Value is > 38 and <= 39  => 1,
            MeasurementType.TEMP when measurement.Value is > 39 and <= 42  => 3,
            
            MeasurementType.HR when measurement.Value is > 25 and <= 40  => 3,
            MeasurementType.HR when measurement.Value is > 40 and <= 50  => 1,
            MeasurementType.HR when measurement.Value is > 50 and <= 90  => 0,
            MeasurementType.HR when measurement.Value is > 90 and <= 110  => 1,
            MeasurementType.HR when measurement.Value is > 110 and <= 130  => 2,
            MeasurementType.HR when measurement.Value is > 130 and <= 220  => 3,
            
            MeasurementType.RR when measurement.Value is > 3 and <= 8  => 3,
            MeasurementType.HR when measurement.Value is > 8 and <= 11  => 1,
            MeasurementType.HR when measurement.Value is > 11 and <= 20  => 0,
            MeasurementType.HR when measurement.Value is > 20 and <= 24  => 2,
            MeasurementType.HR when measurement.Value is > 24 and <= 60  => 3,
            
            _ => null
        };
    }

    private int? CalculateScoreVeryFast(Measurement measurement)
    {
        return MeasurementTypeConfigReader
            .GetInstance(_configuration)
            .MeasurementToScoreMapping
            .GetValueOrDefault(measurement, null);
    }
}