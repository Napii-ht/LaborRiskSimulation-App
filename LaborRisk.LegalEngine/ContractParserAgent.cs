using System;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Math;
using LaborRisk.Domain;
using Microsoft.Extensions.Configuration;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace LaborRisk.LegalEngine
{
    public class ContractParserAgent
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private static string GroqApiKey => Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;

        public ContractParserAgent(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }
        public async Task<ContractAnalysisResult> ExtractDataAsync(string rawText, string apiKey = "")
        {
            string activeKey = !string.IsNullOrWhiteSpace(apiKey)
                                ? apiKey
    :                            (_configuration["GroqApiKey"] ?? _configuration["GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? Environment.GetEnvironmentVariable("GroqApiKey") ?? "");

            if (string.IsNullOrWhiteSpace(rawText))
                return FallbackMock(rawText);
            rawText = System.Text.RegularExpressions.Regex.Replace(rawText, @"\s+", " ").Trim();
            string aiText = "";
            try
            {
                string prompt = @"
Bạn là chuyên gia phân tích hợp đồng lao động Việt Nam theo Bộ luật Lao động 2019.

VỚI MỖI ĐIỀU KHOẢN, HÃY XÁC ĐỊNH 'decision' THEO 3 HƯỚNG:
1. 'DongY': Điều khoản chuẩn xác, công bằng, tuân thủ pháp luật.
2. 'TuChoi': Điều khoản vi phạm điều cấm của pháp luật nghiêm trọng (như phạt tiền, giữ giấy tờ gốc), không thể thỏa thuận.
3. 'DamPhan': Điều khoản KHÔNG sai luật hoàn toàn, nhưng chứa rủi ro, mập mờ hoặc gây bất lợi lớn cho người lao động.
- CỰC KỲ QUAN TRỌNG: Phải đảm bảo đóng đủ tất cả các dấu ngoặc nhọn {} và ngoặc vuông [] của JSON. TUYỆT ĐỐI KHÔNG để JSON bị cắt cụt giữa chừng.

Quy tắt trình bày dữ liệu:
- 'originalText': Trích nguyên văn toàn bộ điều khoản rủi ro, không rút gọn.
- 'strategy': Viết súc tích, ngắn gọn (tối đa 2 câu).
- 'talkingPoints': Viết nguyên văn câu thoại của ứng viên nói với HR/Sếp (xưng 'em', gọi 'anh/chị').

Quy tắc căn cứ pháp lý bắt buộc (Bộ Luật lao động 2019):
NHÓM 1: THỜI GIAN VÀ LƯƠNG THỬ VIỆC
- Thời gian thử việc tối đa 60 ngày (trình độ cao đẳng trở lên): Khoản 2 Điều 25.
- Thời gian thử việc tối đa 30 ngày (trình độ trung cấp, công nhân): Khoản 3 Điều 25.
- Lương thử việc tối thiểu 85% lương chính thức: Điều 26.
- Cấm thử việc đối với HĐLĐ dưới 01 tháng: Khoản 3 Điều 24.

NHÓM 2: PHẠT VI PHẠM VÀ BỒI THƯỜNG CHI PHÍ ĐÀO TẠO
- Cấm phạt tiền, cắt lương thay kỷ luật – Khoản 2 Điều 127.
- Cấm giữ bản chính giấy tờ, văn bằng, chứng chỉ – Khoản 1 Điều 17.
- Cấm yêu cầu đặt cọc tiền, tài sản – Khoản 2 Điều 17.
- Chi phí đào tạo căn cứ Khoản 3 Điều 62.
- Khoản hoàn trả không chứng minh được hoặc vượt chi phí thực tế đưa ra DamPhan, yêu cầu giải trình và chứng từ; không tự động TuChoi.

NHÓM 3: CAM KẾT BẢO MẬT VÀ KHÔNG CẠNH TRANH 
- Thỏa thuận bảo vệ bí mật kinh doanh, bí mật công nghệ – Khoản 2 Điều 21.
- BLLĐ 2019 không quy định thời hạn NCA bắt buộc hoặc bắt buộc trả đền bù.
- NCA quá rộng hoặc hạn chế đáng kể việc làm → DamPhan để thỏa thuận khoản hỗ trợ tài chính trong thời gian bị hạn chế.

NHÓM 4: THỜI GIỜ LÀM VIỆC, LÀM THÊM GIỜ (OT)
- Thời giờ làm việc bình thường (tối đa 8 giờ/ngày, 48 giờ/tuần): Khoản 1 Điều 105.
- Tiền lương làm thêm giờ (Ngày thường >=150%, Nghỉ hàng tuần >=200%, Nghỉ lễ/Tết >=300%): Điểm a, b, c Khoản 1 Điều 98.
- Giới hạn giờ làm thêm (tối đa 40 giờ/tháng, 200 giờ/năm): Điểm b, c Khoản 2 Điều 107.

NHÓM 5: ĐIỀU KIỆN ĐƠN PHƯƠNG CHẤM DỨT HỢP ĐỒNG
- NLĐ đơn phương chấm dứt (Báo trước 45 ngày HĐ vô thời hạn; 30 ngày HĐ 12-36 tháng; 3 ngày HĐ <12 tháng): Điểm a, b, c Khoản 1 Điều 35.
- NLĐ đơn phương chấm dứt KHÔNG cần báo trước (khi bị chậm lương, ngược đãi, vi phạm điều kiện làm việc): Khoản 2 Điều 35.
- NSDLĐ đơn phương chấm dứt (phải thuộc các trường hợp luật định và tuân thủ thời hạn báo trước): Khoản 1 và Khoản 2 Điều 36.

Hãy trả về kết quả dưới dạng ĐÚNG 1 cấu trúc JSON duy nhất (không kèm văn bản giải thích ngoài JSON) theo mẫu chính xác sau:
{
  ""clauses"": [
    {
      ""clauseTitle"": ""Tên điều khoản (VD: Thử việc, Tiền lương, Bồi thường)"",
      ""originalText"": ""Trích nguyên văn câu rủi ro từ hợp đồng"",
      ""decision"": ""DamPhan"",
      ""riskLevel"": ""Cao"",
      ""legalReference"": ""Căn cứ pháp lý (VD: Khoản 1 Điều 25 Bộ luật Lao động 2019)"",
      ""strategy"": {
        ""whyNegotiate"": ""Lý do chi tiết vì sao điều khoản này bất lợi"",
        ""proposedText"": ""Đề xuất lại câu từ mới chuẩn pháp luật"",
        ""talkingPoints"": ""Lời khuyên ngắn gọn để nói chuyện trực tiếp với HR/Sếp""
      }
    }
  ]
}";
                
                // 1. Cấu hình Payload cho Groq API
                var payload = new
                {
                    model = "openai/gpt-oss-120b",
                    messages = new[]
                    {
                        new { role = "system", content = prompt },
                        new { role = "user", content = $"VĂN BẢN HỢP ĐỒNG:\n{rawText}" }
                    },
                    temperature = 0.2,
                    max_tokens = 8192
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Add("Authorization", $"Bearer {activeKey}");
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // 2. Gọi Cloud API
                var response = await _http.SendAsync(request);
                string resContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Groq API trả về mã lỗi HTTP {(int.Parse(response.StatusCode.ToString("D")))}: {resContent}");
                }

                using var doc = JsonDocument.Parse(resContent);

                // Trích xuất nội dung trả về theo chuẩn OpenAI/Groq
                aiText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                string json = aiText.Trim();
                if (json.StartsWith("```json"))
                {
                    json = json.Replace("```json", "").Replace("```", "").Trim();
                }
                else if (json.StartsWith("```"))
                {
                    json = json.Replace("```", "").Trim();
                }

                int firstBrace = json.IndexOf('{');
                if (firstBrace >= 0)
                {
                    json = json.Substring(firstBrace);
                }
                else
                {
                    json = "{\"summary\": \"Hệ thống đang bận hoặc phản hồi từ AI không theo chuẩn JSON. Vui lòng thử lại.\", \"clauses\": []}";
                }

                if (!json.EndsWith("}"))
                {
                    int lastBrace = json.LastIndexOf('}');
                    if (lastBrace > 0)
                    {
                        json = json.Substring(0, lastBrace + 1);
                    }
                    else
                    {
                        json += "]}";
                    }
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var result = JsonSerializer.Deserialize<ContractAnalysisResult>(json, options);
                if (result != null)
                {
                    result.Clauses ??= new List<ClauseItem>();
                        return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION TRONG PARSER]: {ex.Message}");
                return FallbackMock(rawText);

            }
            return FallbackMock(rawText);
        }
        private ContractAnalysisResult FallbackMock(string rawtext)
        {
            rawtext ??= "";
            return new ContractAnalysisResult
            {
                Clauses = new List<ClauseItem>
        {
            new ClauseItem
            {
                ClauseTitle = "Điều khoản thử việc",
                OriginalText = rawtext.Contains("tháng") ? "Điều khoản thử việc trong hợp đồng" : "Thử việc",
                Decision = "Cảnh báo",
                RiskLevel = "Medium",
                LegalReference = "Bộ luật Lao động 2019",
                Strategy = new NegotiationStrategy
                {
                    WhyNegotiate = "Cần làm rõ thời gian và mức lương thử việc theo đúng luật định.",
                    ProposedText = "Đề xuất thời gian thử việc không quá 60 ngày đối với công việc chuyên môn kỹ thuật.",
                    TalkingPoints = "Em muốn xác nhận lại lịch trình và quyền lợi trong giai đoạn thử việc ạ."
                }
            }
        }
            };
        }
    }
}
