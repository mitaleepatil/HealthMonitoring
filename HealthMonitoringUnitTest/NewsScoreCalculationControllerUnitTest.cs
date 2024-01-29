using HealthMonitoring.Controllers;
using HealthMonitoring.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

namespace HealthMonitoringUnitTest;

public class NewsScoreCalculationControllerUnitTest
{
    private readonly NewsScoreCalculationController _newsScoreCalculationController;

    public NewsScoreCalculationControllerUnitTest()
    {
        var projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new[] { @"bin\" }, StringSplitOptions.None)[0];
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.Test.json")
            .Build();
        _newsScoreCalculationController = new NewsScoreCalculationController(config);
    }

    [Fact]
    public void WhenValueIsOutOfRange_Get_OutOfRange_ErrorMessage()
    {
        var measurementsRequest = new MeasurementsRequest(
            new List<Measurement>
            {
                new(MeasurementType.TEMP, 10),
                new(MeasurementType.HR, 60),
                new(MeasurementType.RR, 5)
            }
        );
        var res = _newsScoreCalculationController.CalculateNewsScore(measurementsRequest);

        Assert.IsType<BadRequest<string>>(res.Result);
        dynamic result = res.Result;
        Assert.Equal("Out of Range: 10 for Type: TEMP", result.Value);
    }

    [Fact]
    public void WhenMissingOneOfTypeName_Get_MissingType_ErrorMessage()
    {
        var measurementsRequest = new MeasurementsRequest(
            new List<Measurement>
            {
                new(MeasurementType.TEMP, 37),
                new(MeasurementType.HR, 60),
            }
        );
        var res = _newsScoreCalculationController.CalculateNewsScore(measurementsRequest);

        Assert.IsType<BadRequest<string>>(res.Result);
        dynamic result = res.Result;
        Assert.Equal("Missing Type: RR", result.Value);
    }

    [Fact]
    public void WhenDuplicateTypeName_Get_DuplicateType_ErrorMessage()
    {
        var measurementsRequest = new MeasurementsRequest(
            new List<Measurement>
            {
                new(MeasurementType.TEMP, 37),
                new(MeasurementType.HR, 60),
                new(MeasurementType.RR, 5),
                new(MeasurementType.RR, 6)
            }
        );
        var res = _newsScoreCalculationController.CalculateNewsScore(measurementsRequest);

        Assert.IsType<BadRequest<string>>(res.Result);
        dynamic result = res.Result;
        Assert.Equal("Duplicate Type: RR", result.Value);
    }

    [Fact]
    public void WhenValidInputs_Get_NewsScore()
    {
        var measurementsRequest = new MeasurementsRequest(
            new List<Measurement>
            {
                new(MeasurementType.TEMP, 37),
                new(MeasurementType.HR, 60),
                new(MeasurementType.RR, 5)
            }
        );

        var res = _newsScoreCalculationController.CalculateNewsScore(measurementsRequest);

        Assert.IsType<Ok<ScoreResult>>(res.Result);
        dynamic result = res.Result;
        Assert.Equal(3, result.Value.Score);
    }
}