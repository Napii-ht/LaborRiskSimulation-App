using System.Text.Json;
using LaborRisk.Domain;
using System.Text;
namespace LaborRisk.LegalEngine
{
    public class SimulationService
    {
        public SimulationTurnResult StartScenario(string scenarioId)
        {
            return new SimulationTurnResult
            {
                HrMessage = "Chào bạn! Công ty rất ấn tượng với hồ sơ của bạn. Về hợp đồng lao động, mức lương thử việc sẽ bằng 70% lương chính thức và thời gian thử việc là 90 ngày. Bạn thấy thế nào?",
                CoachAdvice = "Bẫy pháp lý phát hiện: Theo Điều 25 BLLĐ 2019, thử việc trình độ Đại học tối đa 60 ngày. Theo Điều 26, lương thử việc tối thiểu phải bằng 85%. Hãy đề xuất HR điều chỉnh đúng luật.",
                ScoreDelta = 0,
                IsScenarioEnded = false
            };
        }

        public SimulationTurnResult ProcessUserTurn(UserTurnRequest request)
        {
            string text = request.UserResponse.ToLower();

            if (text.Contains("85") || text.Contains("luật") || text.Contains("26") || text.Contains("25") || text.Contains("60 ngày"))
            {
                return new SimulationTurnResult
                {
                    HrMessage = "Cảm ơn bạn đã phản hồi! Công ty ghi nhận và sẽ điều chỉnh lại thời gian thử việc thành 60 ngày với 85% lương chính thức theo đúng Bộ luật Lao động.",
                    CoachAdvice = "Xuất sắc! Bạn đã đàm phán thành công và bảo vệ được quyền lợi hợp pháp của mình.",
                    ScoreDelta = 100,
                    IsScenarioEnded = true
                };
            }

            return new SimulationTurnResult
            {
                HrMessage = "Quy định này là khung chung của công ty dành cho nhân sự mới rồi bạn ạ. Bạn có thể linh hoạt chấp nhận điều khoản này được không?",
                CoachAdvice = "Gợi ý: Hãy nhắc đến 'Điều 26 Bộ luật Lao động 2019' để HR biết bạn hiểu rõ pháp luật và không thể bị ép yếu thế.",
                ScoreDelta = 10,
                IsScenarioEnded = false
            };
        }
        public async Task<UserTurnResponse> ProcessUserTurnAsync(UserTurnRequest request)
        {
            // Gọi sang Ollama Agent hoặc xử lý logic lượt hội thoại
            await Task.Delay(500); // Giả lập xử lý bất đồng bộ

            return new UserTurnResponse
            {
                HrMessage = "Tôi đã ghi nhận ý kiến của bạn. Tuy nhiên điều khoản này cần xem xét thêm.",
                CoachAdvice = "Lời khuyên: Bạn nên đưa ra căn cứ pháp lý rõ ràng hơn để thuyết phục HR.",
                ScoreDelta = 5,
                IsScenarioEnded = false
            };
        }
        public List<RiskQuestionScenario> GetTrainingScenarios()
        {
            return new List<RiskQuestionScenario>
            {
                new RiskQuestionScenario
                {
                    Id = 1,
                    Category = "1. Thời giờ làm việc & Thử việc",
                    ClauseText = "Thời gian thử việc là 03 tháng đối với vị trí chuyên viên kinh doanh.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 25 Bộ luật Lao động 2019, thời gian thử việc cho trình độ chuyên môn kỹ thuật chỉ tối đa 60 ngày (2 tháng)."
                },
                new RiskQuestionScenario
                {
                    Id = 2,
                    Category = "2. Lương & Phụ cấp",
                    ClauseText = "Mức lương thử việc bằng 70% mức lương chính thức.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 28 BLLĐ, tiền lương thử việc phải ít nhất bằng 85% mức lương của công việc đó."
                },
                new RiskQuestionScenario
                {
                    Id = 3,
                    Category = "3. Bảo mật & Cam kết không cạnh tranh (NDA)",
                    ClauseText = "Người lao động không được làm việc cho đối thủ cạnh tranh trong vòng 2 năm sau khi nghỉ việc, nếu vi phạm bồi thường 100 triệu.",
                    IsRiskyClause = true,
                    Explanation = "Điều khoản này hạn chế quyền tự do làm việc Hiến định. Cần đàm phán lại khoản phụ cấp bù đắp bảo mật."
                },
                new RiskQuestionScenario
                {
                    Id = 4,
                    Category = "4. Bồi thường chi phí đào tạo",
                    ClauseText = "Nếu người lao động đơn phương chấm dứt hợp đồng đúng luật vẫn phải hoàn trả 100% chi phí đào tạo nội bộ.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 62 BLLĐ, chỉ phải hoàn trả chi phí đào tạo nếu đơn phương chấm dứt hợp đồng trái pháp luật."
                },
                new RiskQuestionScenario
                {
                    Id = 5,
                    Category = "5. Chấm dứt Hợp đồng lao động",
                    ClauseText = "Công ty có quyền đơn phương chấm dứt hợp đồng mà không cần báo trước nếu nhân viên không đạt KPI 1 tháng.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 36 BLLĐ, người sử dụng lao động phải báo trước ít nhất 30 ngày (với HĐLĐ xác định thời hạn từ 12-36 tháng)."
                }
            };
        }
        public async Task<string> EvaluateDecisionWithAIAsync(string clause, bool userChoice, bool isRisky)
        {
            string actionText = userChoice ? "ĐỒNG Ý KÝ" : "TỪ CHỐI / YÊU CẦU ĐÀM PHÁN";

            try
            {
                // 1. Giới hạn thời gian chờ AI phản hồi trong tối đa 5 giây để tránh treo giao diện
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

                var payload = new
                {
                    model = "qwen2.5:7b-instruct-q4_K_M",
                    prompt = $"Đóng vai Huấn luyện viên Pháp lý Lao động. Điều khoản: '{clause}'. Người dùng chọn: '{actionText}'. " +
                             $"Hãy đưa ra phản hồi ngắn gọn (2-3 câu) nhận xét lựa chọn của họ có an toàn không và lời khuyên thực tế khi đi ký hợp đồng.",
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // 2. Thực hiện gọi API Ollama
                var response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    return doc.RootElement.GetProperty("response").GetString() ?? "Đã nhận diện lựa chọn thành công.";
                }
            }
            catch (Exception)
            {
                // Khi Ollama không bật hoặc phản hồi quá lâu, catch sẽ tự động kích hoạt phản hồi mặc định ngay lập tức
            }

            // 3. Phản hồi dự phòng an toàn (Backup) giúp nút bấm xử lý tức thì
            bool isCorrect = (userChoice == !isRisky);
            return isCorrect
                ? "Lựa chọn chính xác! Bạn đã phát hiện đúng trạng thái rủi ro của điều khoản này."
                : "Cảnh báo: Quyết định này có thể mang lại rủi ro pháp lý cho bạn khi đi làm.";
        }
    }
}