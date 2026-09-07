using LaborRisk.Domain;
using LaborRisk.LegalEngine;
using Xunit;

namespace LaborRisk.Tests
{
    public class RuleMatrixTests
    {
        private readonly LegalRuleEngine _engine = new();

        // 1. Test trường hợp thử việc quá 60 ngày
        [Fact]
        public void EvaluateContract_ProbationExceeded_ShouldReturnViolation()
        {
            // Arrange (Chuẩn bị dữ liệu)
            var contract = new ContractInput
            {
                ProbationDays = 90 // Vi phạm: trần chỉ 60 ngày
            };

            // Act (Thực thi kiểm tra)
            var result = _engine.EvaluateContract(contract);

            // Assert (Kiểm tra kết quả)
            Assert.NotEmpty(result.Risks);
            Assert.Contains(result.Risks, r => r.Title.Contains("Thử việc"));
        }

        // 2. Test trường hợp lương thử việc thấp hơn 85%
        [Fact]
        public void EvaluateContract_ProbationSalaryLow_ShouldReturnViolation()
        {
            // Arrange
            var contract = new ContractInput
            {
                BaseSalary = 10000000,
                ProbationSalary = 7000000 // 70% < 85% luật định
            };

            // Act
            var result = _engine.EvaluateContract(contract);

            // Assert
            Assert.NotEmpty(result.Risks);
            Assert.Contains(result.Risks, r => r.Title.Contains("Lương thử việc"));
        }

        // 3. Test trường hợp giữ bằng gốc và phạt tiền
        [Fact]
        public void EvaluateContract_HoldsDegreeAndPenalty_ShouldReturnMultipleViolations()
        {
            // Arrange
            var contract = new ContractInput
            {
                HasDegreeRetention = true,
                PenaltyAmount = 2000000
            };

            // Act
            var result = _engine.EvaluateContract(contract);

            // Assert
            Assert.Equal(2, result.Risks.Count);
            Assert.True(result.LriScore <= 50); // Trừ 50 điểm LRI
        }
    }
}