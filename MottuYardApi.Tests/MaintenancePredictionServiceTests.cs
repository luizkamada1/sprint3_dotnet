using MottuYardApi.Services;
using Xunit;

namespace MottuYardApi.Tests;

public class MaintenancePredictionServiceTests
{
    [Fact]
    public void Predict_Should_Return_Result()
    {
        var service = new MaintenancePredictionService();
        var input = new MaintenanceData
        {
            DaysSinceMaintenance = 45,
            CompletedDeliveries = 150,
            BreakdownHistory = 3
        };

        var prediction = service.Predict(input);

        Assert.True(prediction.Probability is >= 0 and <= 1);
        Assert.True(prediction.RequiresMaintenance);
    }

    [Fact]
    public void Predict_Should_Throw_For_Null_Input()
    {
        var service = new MaintenancePredictionService();

        Assert.Throws<ArgumentNullException>(() => service.Predict(null!));
    }
}
