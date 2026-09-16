using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using LaborRisk.LegalEngine; // Gọi sang project chứa Agent phân tích
using LaborRisk.Domain;      // Gọi sang project chứa Model

namespace LaborRisk.Tests.Benchmarks
{
    public class BenchmarkRunnerTests
    {
        private readonly ContractParserAgent _parserAgent;

        public BenchmarkRunnerTests()
        {
            // Khởi tạo Agent trực tiếp từ project LaborRisk.LegalEngine 
            var httpClient = new HttpClient();
            _parserAgent = new ContractParserAgent(httpClient);
        }

        [Theory]
        [MemberData(nameof(GetAllBenchmarkFiles))]
        public async Task EvaluateScenario_ShouldMatchGroundTruth(string filePath)
        {
            // 1. Đọc nội dung file JSON
            string jsonContent = await File.ReadAllTextAsync(filePath);
            var scenario = JsonSerializer.Deserialize<BenchmarkScenario>(jsonContent);

            Assert.NotNull(scenario);
            Assert.NotNull(scenario.GroundTruth);

            // 2. Kiểm tra kết quả
            var result = await _parserAgent.ExtractDataAsync(scenario.RawText);
            Assert.NotNull(result);
            var expectedTruth = scenario.GroundTruth.FirstOrDefault();
            Assert.NotNull(expectedTruth);
            Assert.NotNull(result.Clauses);
            Assert.True(result.Clauses.Count > 0);

        }

        // Tự động quét 20 file .json trong thư mục Benchmarks
        public static IEnumerable<object[]> GetAllBenchmarkFiles()
        {
            string benchmarkDir = Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks");
            if (Directory.Exists(benchmarkDir))
            {
                var files = Directory.GetFiles(benchmarkDir, "*.json");
                foreach (var file in files)
                {
                    yield return new object[] { file };
                }
            }
        }
    }

    // Các Class DTO hứng dữ liệu từ file JSON (Khai báo ngay bên dưới)
    public class BenchmarkScenario
    {
        public string ScenarioId { get; set; }
        public string Category { get; set; }
        public string ContractTitle { get; set; }
        public string TargetPosition { get; set; }
        public string EducationLevelRequired { get; set; }
        public string RawText { get; set; }
        public List<GroundTruthItem> GroundTruth { get; set; }
    }

    public class GroundTruthItem
    {
        public string ClauseId { get; set; }
        public string ClauseTitle { get; set; }
        public string OriginalText { get; set; }
        public string ExpectedDecision { get; set; }
        public string ExpectedRiskLevel { get; set; }
        public string ExpectedLegalReference { get; set; }
        public string KeyViolationReason { get; set; }
    }
}