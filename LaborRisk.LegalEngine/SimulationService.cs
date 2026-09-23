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
        public List<RiskQuestionScenario> GetTrainingScenarios()
        {
            return new List<RiskQuestionScenario>
            {
                new RiskQuestionScenario
                {
                    Id = 1,
                    Category = "1. Thời giờ làm việc và thử việc",
                    ClauseText = "Thời gian thử việc là 03 tháng đối với vị trí chuyên viên kinh doanh.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 25 BLLĐ 2019, thời gian thử việc tối đa 60 ngày đối với công việc yêu cầu trình độ từ cao đẳng trở lên."
                },
                new RiskQuestionScenario
                {
                    Id = 2,
                    Category = "2. Lương và Phụ cấp",
                    ClauseText = "Mức lương thử việc bằng 70% mức lương chính thức.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 26 BLLĐ 2019, lương thử việc phải ít nhất bằng 85% lương của công việc đó."
                },
                new RiskQuestionScenario
                {
                    Id = 3,
                    Category = "3. Bảo mật và Cam kết không cạnh tranh (NDA)",
                    ClauseText = "Người lao động không được làm việc cho đối thủ cạnh tranh trong vòng 2 năm sau khi nghỉ việc, nếu vi phạm bồi thường 100 triệu.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 21 BLLĐ 2019, thỏa thuận bảo mật và hạn chế cạnh tranh cần được xem xét về phạm vi, thời hạn và quyền lợi của người lao động."
                },
                new RiskQuestionScenario
                {
                    Id = 4,
                    Category = "4. Bồi thường chi phí đào tạo",
                    ClauseText = "Nếu người lao động đơn phương chấm dứt hợp đồng đúng luật vẫn phải hoàn trả 100% chi phí đào tạo nội bộ.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 62 BLLĐ 2019, nghĩa vụ hoàn trả chi phí đào tạo phải căn cứ vào hợp đồng đào tạo và thỏa thuận giữa các bên."
                },
                new RiskQuestionScenario
                {
                    Id = 5,
                    Category = "5. Chấm dứt Hợp đồng lao động",
                    ClauseText = "Công ty có quyền đơn phương chấm dứt hợp đồng mà không cần báo trước nếu nhân viên không đạt KPI 1 tháng.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 36 BLLĐ 2019, không đạt KPI 1 tháng không mặc nhiên là căn cứ để chấm dứt hợp đồng ngay."
                },
                new RiskQuestionScenario
                {
                    Id = 6,
                    Category = "1. Thời giờ làm việc & Thử việc",
                    ClauseText = "Vị trí yêu cầu trình độ đại học được thử việc 60 ngày; nếu chưa đánh giá đủ năng lực, công ty được gia hạn thêm 15 ngày.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 25 BLLĐ 2019, mỗi công việc chỉ được thử việc một lần và tối đa 60 ngày với công việc yêu cầu trình độ từ cao đẳng trở lên."
                },
                new RiskQuestionScenario
                {
                    Id = 7,
                    Category = "2. Lương và Phụ cấp",
                    ClauseText = "Mức lương chính thức là 14 triệu đồng/tháng, mức lương trong thời gian thử việc là 12 triệu đồng/tháng.",
                    IsRiskyClause = false,
                    Explanation = "Theo Điều 26 BLLĐ 2019, lương thử việc ít nhất bằng 85% lương của công việc; mức 12 triệu đáp ứng yêu cầu."
                },
                new RiskQuestionScenario
                {
                    Id = 8,
                    Category = "1. Thời giờ làm việc & Thử việc",
                    ClauseText = "Người lao động ký hợp đồng làm việc thời vụ 20 ngày và phải thử việc 05 ngày đầu.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 24 BLLĐ 2019, không áp dụng thử việc đối với HĐLĐ có thời hạn dưới 01 tháng."
                },
                new RiskQuestionScenario
                {
                    Id = 9,
                    Category = "2. Lương và Phụ cấp",
                    ClauseText = "Nội quy quy định nhân viên vi phạm lần đầu bị phạt 300.000 đồng và lần hai bị phạt 600.000 đồng.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 127 BLLĐ 2019, không được phạt tiền hoặc cắt lương thay cho xử lý kỷ luật."
                },
                new RiskQuestionScenario
                {
                    Id = 10,
                    Category = "2. Lương và Phụ cấp",
                    ClauseText = "Nhân viên làm hỏng thiết bị của công ty; sau khi xác định trách nhiệm, công ty khấu trừ 20% tiền lương thực trả mỗi tháng để bồi thường.",
                    IsRiskyClause = false,
                    Explanation = "Điều 102 BLLĐ 2019 cho phép khấu trừ để bồi thường thiệt hại, với mức khấu trừ hằng tháng không quá 30% lương thực trả."
                },
                new RiskQuestionScenario
                {
                    Id = 11,
                    Category = "4. Bồi thường chi phí đào tạo",
                    ClauseText = "Công ty tài trợ khóa học bên ngoài và ký hợp đồng đào tạo ghi rõ chi phí, thời gian cam kết làm việc và trách nhiệm hoàn trả.",
                    IsRiskyClause = false,
                    Explanation = "Điều 62 BLLĐ 2019 cho phép các bên thỏa thuận về chi phí đào tạo, thời gian cam kết và trách nhiệm hoàn trả."
                },

                new RiskQuestionScenario
                {
                    Id = 12,
                    Category = "3. Bảo mật và Cam kết không cạnh tranh (NDA)",
                    ClauseText = "Nhân viên trực tiếp tiếp cận bí mật kinh doanh phải bảo mật trong 18 tháng sau khi nghỉ việc theo thỏa thuận bằng văn bản.",
                    IsRiskyClause = false,
                    Explanation = "Khoản 2 Điều 21 BLLĐ 2019 cho phép thỏa thuận bằng văn bản về bảo vệ bí mật kinh doanh, bí mật công nghệ."
                },
                new RiskQuestionScenario
                {
                    Id = 13,
                    Category = "3. Bảo mật và Cam kết không cạnh tranh (NDA)",
                    ClauseText = "Trong 03 năm sau khi nghỉ việc, nhân viên không được làm cho bất kỳ doanh nghiệp nào cùng ngành trên toàn Việt Nam.",
                    IsRiskyClause = true,
                    Explanation = "Điều khoản có phạm vi và thời hạn hạn chế rất rộng nên cần xem xét cụ thể về lợi ích cần bảo vệ và quyền lợi người lao động."
                },

                new RiskQuestionScenario
                {
                    Id = 14,
                    Category = "2. Lương và Phụ cấp",
                    ClauseText = "Mức lương 16 triệu đồng/tháng đã bao gồm toàn bộ tiền làm thêm giờ, không giới hạn số giờ làm thêm.",
                    IsRiskyClause = true,
                    Explanation = "Tiền làm thêm và giới hạn thời giờ làm thêm vẫn phải tuân thủ các quy định của BLLĐ, không thể loại bỏ bằng mức lương cố định."
                },
                new RiskQuestionScenario
                {
                    Id = 15,
                    Category = "5. Chấm dứt Hợp đồng lao động",
                    ClauseText = "Người lao động ký hợp đồng 24 tháng chỉ được nghỉ trước hạn khi có sự chấp thuận của công ty.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 35 BLLĐ 2019, người lao động có quyền đơn phương chấm dứt HĐLĐ khi đáp ứng quy định về báo trước."
                },
                new RiskQuestionScenario
                {
                    Id = 16,
                    Category = "1. Thời giờ làm việc và thử việc",
                    ClauseText = "Nhân viên thử việc 60 ngày với mức lương bằng 80% lương chính thức và đồng ý thử thêm 30 ngày nếu chưa đạt yêu cầu.",
                    IsRiskyClause = true,
                    Explanation = "Điều khoản vừa thấp hơn mức lương thử việc tối thiểu 85%, vừa vượt giới hạn thử việc đối với một công việc."
                },
                new RiskQuestionScenario
                {
                    Id = 17,
                    Category = "4. Bồi thường chi phí đào tạo",
                    ClauseText = "Nhân viên nghỉ việc đúng luật phải nộp phạt 5 triệu đồng và hoàn trả chi phí đào tạo theo hợp đồng đào tạo riêng.",
                    IsRiskyClause = true,
                    Explanation = "Khoản phạt nghỉ việc và nghĩa vụ hoàn trả chi phí đào tạo là hai vấn đề khác nhau; chi phí đào tạo phải xét theo Điều 62 BLLĐ 2019."
                },
                new RiskQuestionScenario
                {
                    Id = 18,
                    Category = "3. Bảo mật và Cam kết không cạnh tranh (NDA)",
                    ClauseText = "Lập trình viên phải bảo mật mã nguồn 24 tháng và không được làm cho bất kỳ công ty công nghệ nào tại Việt Nam trong 03 năm.",
                    IsRiskyClause = true,
                    Explanation = "Cần tách nghĩa vụ bảo mật khỏi hạn chế cạnh tranh; phạm vi cấm làm việc quá rộng là dấu hiệu cần xem xét."
                },

                new RiskQuestionScenario
                {
                    Id = 19,
                    Category = "1. Thời giờ làm việc và thử việc",
                    ClauseText = "Nhân viên làm từ 8 giờ đến 17 giờ và thường xuyên ở lại đến 21 giờ nhưng chỉ nhận lương cố định vì đã đồng ý làm thêm khi cần.",
                    IsRiskyClause = true,
                    Explanation = "Việc đã đồng ý làm thêm không loại trừ nghĩa vụ tuân thủ giới hạn thời giờ và trả tiền làm thêm theo quy định."
                },
                new RiskQuestionScenario
                {
                    Id = 20,
                    Category = "5. Chấm dứt Hợp đồng lao động",
                    ClauseText = "Dù công ty liên tục trả lương chậm, nhân viên vẫn bắt buộc phải báo trước 30 ngày nếu muốn nghỉ việc.",
                    IsRiskyClause = true,
                    Explanation = "Theo Điều 35 BLLĐ 2019, có trường hợp người lao động không được trả đủ hoặc đúng hạn có thể nghỉ mà không cần báo trước."
                }
            };
        }
        public async Task<SimulationTurnResult> ProcessUserNegotiationAsync(string clauseText, string legalExplanation, string userResponse)
        {
            // Thử gọi AI (Ollama) trước để nhận xét linh hoạt
            string aiFeedback = await EvaluateUserNegotiationWithAIAsync(clauseText, legalExplanation, userResponse);

            return new SimulationTurnResult
            {
                HrMessage = "HR đã ghi nhận câu phản hồi của bạn và đang cân nhắc điều chỉnh.",
                CoachAdvice = aiFeedback,
                ScoreDelta = 50,
                IsScenarioEnded = true
            };
        }
        private async Task<string> EvaluateUserNegotiationWithAIAsync(string clauseText, string legalExplanation, string userResponse)
        {
            try
            {
                // 1. Giới hạn thời gian chờ AI phản hồi trong tối đa 60 giây
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                string prompt = $@"Bạn là một Chuyên gia Pháp lý Lao động và Cố vấn Đàm phán Nhân sự cao cấp.
Bối cảnh điều khoản hợp đồng cần đàm phán: '{clauseText}'.
Căn cứ pháp lý chuẩn: '{legalExplanation}'.

Người lao động vừa đưa ra câu thoại đàm phán với HR như sau:
""{userResponse}""

Hãy viết một đoạn nhận xét và đưa ra lời khuyên hoàn chỉnh (khoảng 4-5 câu) bằng tiếng Việt với phong cách chuyên nghiệp:
1. Đánh giá câu thoại có an toàn, khéo léo, xưng hô đúng mực ('em - anh/chị'), giữ hòa khí và tránh dùng luật áp đảo cứng nhắc chưa.
2. Nhắc nhở cụ thể căn cứ từ Bộ luật Lao động 2019 bảo vệ quyền lợi hợp pháp.
3. Gợi ý hướng tinh chỉnh câu chữ tốt nhất.";

                var payload = new
                {
                    model = "qwen2.5:7b-instruct-q4_K_M",
                    prompt = prompt,
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
                else
                {
                    // In ra mã lỗi khi Ollama phản hồi lỗi
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"[LỖI OLLAMA HTTP]: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                return $"[LỖI EXCEPTION]: {ex.Message}";
            }

            // 3. Phản hồi dự phòng an toàn (Backup) giúp nút bấm xử lý tức thì
            string text = userResponse.ToLower();

            if (text.Contains("85") || text.Contains("luật") || text.Contains("26") || text.Contains("25") || text.Contains("60 ngày"))
            {
                return "Nhận xét dự phòng (Hệ thống Offline): Bạn đã nhắc khéo léo đến các quy định chuẩn của pháp luật để bảo vệ quyền lợi rất tốt.";
            }

            return $"Nhận xét dự phòng (Hệ thống Offline): Lời đàm phán của bạn cần chú ý giữ hòa khí, xưng hô 'em - anh/chị' nhẹ nhàng và lưu ý căn cứ pháp lý sau: {legalExplanation}";
        }
    }
    
}