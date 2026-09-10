using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LaborRisk.Domain
{
    // 1. Dữ liệu hợp đồng do AI bóc tách từ file Word/PDF
    public class ContractInput
    {
        [JsonPropertyName("probationDays")]
        public int ProbationDays { get; set; }

        [JsonPropertyName("baseSalary")]
        public long BaseSalary { get; set; }

        [JsonPropertyName("probationSalary")]
        public long ProbationSalary { get; set; }

        [JsonPropertyName("penaltyAmount")]
        public long PenaltyAmount { get; set; }

        [JsonPropertyName("overtimeMultiplier")]
        public double OvertimeMultiplier { get; set; }

        [JsonPropertyName("noticeDaysEmployee")]
        public int NoticeDaysEmployee { get; set; }

        [JsonPropertyName("hasVagueJobDescription")]
        public bool HasVagueJobDescription { get; set; }

        [JsonPropertyName("hasDegreeRetention")]
        public bool HasDegreeRetention { get; set; }
    }

    // 2. Thông tin bẫy pháp lý phát hiện
    public class RiskItem
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LawReference { get; set; } = string.Empty;
        public int DeductedScore { get; set; }
    }

    // 3. Kết quả đánh giá tổng hợp LRI
    public class AssessmentResult
    {
        public int LriScore { get; set; }
        public List<RiskItem> Risks { get; set; } = new List<RiskItem>();
        public string AssessmentLevel => LriScore switch
        {
            >= 80 => "AN TOÀN - Hợp đồng hợp lệ",
            >= 50 => "CẢNH BÁO - Chứa điều khoản bất lợi, cần đàm phán lại!",
            _ => "NGUY HIỂM - Vi phạm nghiêm trọng, TUYỆT ĐỐI KHÔNG KÝ!"
        };
    }

    // 4. Trạng thái điều khoản hợp đồng dùng cho mô phỏng
    public class ContractEntity
    {
        public int ProbationDays { get; set; }
        public double ProbationSalaryRate { get; set; }
    }

    // 5. Bối cảnh người dùng
    public class UserContext
    {
        public string QualificationLevel { get; set; } = "DaiHoc";
    }

    // 6. Yêu cầu lượt đàm phán gửi lên SimulationService
    public class UserTurnRequest
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string UserResponse { get; set; } = string.Empty;
        public ContractEntity ContractState { get; set; } = new ContractEntity();
        public UserContext UserContext { get; set; } = new UserContext();
    }
    public class UserTurnResponse
    {
        public string HrMessage { get; set; } = string.Empty;
        public string CoachAdvice { get; set; } = string.Empty;
        public int ScoreDelta { get; set; }
        public bool IsScenarioEnded { get; set; }
    }
    // 7. Kết quả phản hồi từ SimulationService
    public class SimulationTurnResult
    {
        public string HrMessage { get; set; } = string.Empty;
        public string CoachAdvice { get; set; } = string.Empty;
        public int ScoreDelta { get; set; }
        public bool IsScenarioEnded { get; set; }
    }
    public class RiskQuestionScenario
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ClauseText { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public bool IsRiskyClause { get; set; }
        public int Points { get; set; } = 20;
        public string Explanation { get; set; } = string.Empty;
    }
    public class ContractAnalysisResult
    {
        public List<ClauseItem> Clauses { get; set; } = new();
    }

    public class ClauseItem
    {
        public string ClauseTitle { get; set; } = string.Empty;       // Tên điều khoản (VD: Thử việc, Lương, Bồi thường)
        public string OriginalText { get; set; } = string.Empty;      // Nội dung gốc
        public string Decision { get; set; } = string.Empty;          // "DongY", "TuChoi", hoặc "DamPhan"
        public string RiskLevel { get; set; } = string.Empty;         // "Thap", "TrungBinh", "Cao"
        public string LegalReference { get; set; } = string.Empty;     // Căn cứ pháp lý (Bộ luật Lao động 2019)
        public NegotiationStrategy Strategy { get; set; } = new();   // Kịch bản đàm phán
    }
    public class NegotiationStrategy
    {
        public string WhyNegotiate { get; set; } = string.Empty;      // Lý do bất lợi dù có thể không sai luật
        public string ProposedText { get; set; } = string.Empty;      // Đề xuất câu từ hợp đồng mới
        public string TalkingPoints { get; set; } = string.Empty;     // Lời khuyên/Gợi ý câu từ khi đàm phán
    }
    public class HighlightedContractModel
    {
        public string FullTextHtml { get; set; } = string.Empty; // Văn bản hợp đồng chứa thẻ HTML tô đậm
        public List<ClauseItem> RiskyClauses { get; set; } = new();
    }
}