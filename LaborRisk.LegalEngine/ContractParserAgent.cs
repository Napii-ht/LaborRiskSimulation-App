using System;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Math;
using LaborRisk.Domain;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace LaborRisk.LegalEngine
{
    public class ContractParserAgent
    {
        private readonly HttpClient _http;
        private static string GroqApiKey => Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;

        public ContractParserAgent(HttpClient http)
        {
            _http = http;
        }

        public async Task<ContractInput> ExtractDataAsync(string rawText, string apiKey = "")
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return FallbackMock(rawText);

            try
            {
                string prompt = @"
Bạn là Chuyên gia Pháp lý và Cố vấn Đàm phán Hợp đồng Lao động theo Bộ luật Lao động Việt Nam 2019.
Nhiệm vụ của bạn là phân tích hợp đồng được cung cấp và đưa ra cố vấn chiến lược cho từng điều khoản.

VỚI MỖI ĐIỀU KHOẢN, HÃY XÁC ĐỊNH 'Decision' THEO 3 HƯỚNG:
1. 'DongY': Điều khoản chuẩn xác, công bằng, tuân thủ pháp luật.
2. 'TuChoi': Điều khoản vi phạm điều cấm của pháp luật nghiêm trọng, không thể thỏa thuận.
3. 'DamPhan': Điều khoản KHÔNG sai luật hoàn toàn, nhưng chứa rủi ro, mập mờ hoặc gây bất lợi lớn cho người lao động.

Hãy trả về kết quả dưới dạng ĐÚNG 1 cấu trúc JSON duy nhất (không kèm văn bản giải thích ngoài JSON) theo mẫu:
{
  ""Clauses"": [
    {
      ""ClauseTitle"": ""Tên điều khoản (VD: Thử việc, Tiền lương, Bồi thường)"",
      ""OriginalText"": ""Nội dung gốc trong hợp đồng"",
      ""Decision"": ""DongY"",
      ""RiskLevel"": ""Thap"",
      ""LegalReference"": ""Điều 25 Bộ luật Lao động 2019"",
      ""Strategy"": {
        ""WhyNegotiate"": ""Lý do chi tiết vì sao nên đàm phán lại"",
        ""ProposedText"": ""Đoạn văn bản hợp đồng đề xuất sửa lại"",
        ""TalkingPoints"": ""Gợi ý kịch bản lời nói khi thương lượng với sếp""
      }
    }
  ]
}";

                // 1. Cấu hình Payload cho Groq API (Qwen 2.5)
                var payload = new
                {
                    model = "qwen-2.5-32b",
                    messages = new[]
                    {
                        new { role = "system", content = prompt },
                        new { role = "user", content = $"VĂN BẢN HỢP ĐỒNG:\n{rawText}" }
                    },
                    temperature = 0.2
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

                // Dùng Key truyền vào hoặc Key mặc định
                string activeKey = string.IsNullOrWhiteSpace(apiKey) ? GroqApiKey : apiKey;
                request.Headers.Add("Authorization", $"Bearer {activeKey}");
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // 2. Gọi Cloud API
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string resContent = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(resContent);

                    // Trích xuất nội dung trả về theo chuẩn OpenAI/Groq
                    string aiText = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "";

                    var match = System.Text.RegularExpressions.Regex.Match(aiText, @"\{.*\}", System.Text.RegularExpressions.RegexOptions.Singleline);
                    string json = match.Success ? match.Value : aiText;

                    var result = JsonSerializer.Deserialize<ContractInput>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null) return result;
                }
            }
            catch (Exception)
            {
                // Tự động dùng Fallback nếu gặp sự cố mạng
            }

            return FallbackMock(rawText);
        }

        private ContractInput FallbackMock(string text)
        {
            text ??= "";
            return new ContractInput
            {
                ProbationDays = text.Contains("90 ngày") || text.Contains("03 tháng") ? 90 : 30,
                BaseSalary = 15000000,
                ProbationSalary = text.Contains("70%") ? 10500000 : 12750000,
                PenaltyAmount = text.Contains("phạt tiền") ? 3000000 : 0,
                OvertimeMultiplier = text.Contains("100%") ? 1.0 : 1.5,
                NoticeDaysEmployee = text.Contains("60 ngày") ? 60 : 30,
                HasVagueJobDescription = text.Contains("không giới hạn") || text.Contains("phân công khác"),
                HasDegreeRetention = text.Contains("bản chính") || text.Contains("bằng gốc")
            };
        }
    }
}
