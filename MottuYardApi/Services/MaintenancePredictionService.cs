using Microsoft.ML;
using Microsoft.ML.Data;

namespace MottuYardApi.Services
{
    public class MaintenanceData
    {
        [LoadColumn(0)]
        public float DaysSinceMaintenance { get; set; }

        [LoadColumn(1)]
        public float CompletedDeliveries { get; set; }

        [LoadColumn(2)]
        public float BreakdownHistory { get; set; }

        [LoadColumn(3)]
        public bool RequiresMaintenance { get; set; }
    }

    public class MaintenancePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool RequiresMaintenance { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }

    public class MaintenancePredictionService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public MaintenancePredictionService()
        {
            _mlContext = new MLContext(seed: 42);
            var trainingData = _mlContext.Data.LoadFromEnumerable(BuildTrainingSet());
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(MaintenanceData.DaysSinceMaintenance),
                    nameof(MaintenanceData.CompletedDeliveries),
                    nameof(MaintenanceData.BreakdownHistory))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());

            _model = pipeline.Fit(trainingData);
        }

        public MaintenancePrediction Predict(MaintenanceData input)
        {
            ArgumentNullException.ThrowIfNull(input);
            using var engine = _mlContext.Model.CreatePredictionEngine<MaintenanceData, MaintenancePrediction>(_model);
            return engine.Predict(input);
        }

        private static IEnumerable<MaintenanceData> BuildTrainingSet()
        {
            yield return new MaintenanceData { DaysSinceMaintenance = 5, CompletedDeliveries = 40, BreakdownHistory = 0, RequiresMaintenance = false };
            yield return new MaintenanceData { DaysSinceMaintenance = 30, CompletedDeliveries = 120, BreakdownHistory = 2, RequiresMaintenance = true };
            yield return new MaintenanceData { DaysSinceMaintenance = 20, CompletedDeliveries = 80, BreakdownHistory = 1, RequiresMaintenance = true };
            yield return new MaintenanceData { DaysSinceMaintenance = 7, CompletedDeliveries = 60, BreakdownHistory = 0, RequiresMaintenance = false };
            yield return new MaintenanceData { DaysSinceMaintenance = 45, CompletedDeliveries = 150, BreakdownHistory = 3, RequiresMaintenance = true };
            yield return new MaintenanceData { DaysSinceMaintenance = 12, CompletedDeliveries = 50, BreakdownHistory = 0, RequiresMaintenance = false };
            yield return new MaintenanceData { DaysSinceMaintenance = 60, CompletedDeliveries = 200, BreakdownHistory = 4, RequiresMaintenance = true };
            yield return new MaintenanceData { DaysSinceMaintenance = 3, CompletedDeliveries = 30, BreakdownHistory = 0, RequiresMaintenance = false };
        }
    }
}
