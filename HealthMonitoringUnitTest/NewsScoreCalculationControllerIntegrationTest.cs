using System.Net;
using System.Text;
using HealthMonitoring.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace HealthMonitoringUnitTest;

public class NewsScoreCalculationControllerIntegrationTest
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _url = "/measurements";
    
    public NewsScoreCalculationControllerIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WhenValidInput_ReturnsNewsScore()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "type": "temp", "value": 37 }, { "type": "HR", "value": 60 }, { "type": "RR", "value": 5 } ] }""";
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        var res = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ScoreResult>(res);
        Assert.Equal(3, result?.Score);
    }

    [Fact]
    public async Task WhenTypeNameIsMissing_ReturnsInvalidType_ErrorMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "value": 37 }, { "type": "HR", "value": 60 }, { "type": "RR", "value": 5 } ] }""";
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Invalid Type: 0,Missing Type: TEMP\"", result);
    }
    
    [Fact]
    public async Task WhenValueIsMissing_ReturnsOutOfRange_ErrorMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "type": "TEMP" }, { "type": "HR", "value": 60 }, { "type": "RR", "value": 5 } ] }"""; 
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Out of Range: 0 for Type: TEMP\"", result);
    }
    
    [Fact]
    public async Task WhenInvalidMeasurementType_ReturnsInvalidMeasurementType_ErrorMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "type": "TEMP", "value": 37 }, { "type": "HR", "value": 60 }, { "type": "RR", "value": 5 }, { "type": "Invalid", "value": 5 } ] }""";
             
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("The measurementsRequest field is required.", result);
    }
    
    [Fact]
    public async Task WhenMeasurementIsMissing_ReturnsMissingType_ErrorMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "type": "TEMP", "value": 37 }, { "type": "RR", "value": 60 } ] }""";
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Missing Type: HR\"", result);
    }
    
    [Fact]
    public async Task WhenMeasurementIsOutOfRange_ReturnsOutOfRange_ErrorMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        const string json = """{ "measurements": [ { "type": "TEMP", "value": 31 }, { "type": "HR", "value": 60 }, { "type": "RR", "value": 5 } ] }""";
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(_url, data);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Out of Range: 31 for Type: TEMP\"", result);
    }
}