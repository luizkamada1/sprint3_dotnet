using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MottuYardApi.DTOs;
using MottuYardApi.Security;
using Xunit;

namespace MottuYardApi.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Patios_Endpoint_Should_Return_Data()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/v1/patios?page=1&pageSize=2");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PatioDto>>();

        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
    }

    [Fact]
    public async Task Missing_ApiKey_Should_Return_Unauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/patios");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_Check_Should_Be_Available()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Maintenance_Prediction_Should_Return_Success()
    {
        var client = CreateAuthenticatedClient();
        var payload = new MaintenancePredictionRequest(30, 120, 2);

        var response = await client.PostAsJsonAsync("/api/v1/analytics/maintenance-prediction", payload);

        response.EnsureSuccessStatusCode();
        var prediction = await response.Content.ReadFromJsonAsync<MaintenancePredictionResponse>();

        Assert.NotNull(prediction);
        Assert.True(prediction!.Probability is >= 0 and <= 1);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyConstants.HeaderName, "local-dev-key");
        return client;
    }
}
