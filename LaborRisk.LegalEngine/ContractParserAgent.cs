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

        public async Task<ContractAnalysisResult> ExtractDataAsync(string rawText, string apiKey = "")
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            }
            if (string.IsNullOrWhiteSpace(rawText))
                return FallbackMock(rawText);
            string aiText = "";
            try
            {
                string prompt = @"
Bạn là Chuyên gia Pháp lý và Cố vấn Đàm phán Hợp đồng Lao động theo Bộ luật Lao động Việt Nam 2019.
Nhiệm vụ của bạn là phân tích hợp đồng được cung cấp và đưa ra cố vấn chiến lược mang tính thực chiến cao cho từng điều khoản.

VỚI MỖI ĐIỀU KHOẢN, HÃY XÁC ĐỊNH 'Decision' THEO 3 HƯỚNG:
1. 'DongY': Điều khoản chuẩn xác, công bằng, tuân thủ pháp luật.
2. 'TuChoi': Điều khoản vi phạm điều cấm của pháp luật nghiêm trọng (như phạt tiền, giữ giấy tờ gốc), không thể thỏa thuận.
3. 'DamPhan': Điều khoản KHÔNG sai luật hoàn toàn, nhưng chứa rủi ro, mập mờ hoặc gây bất lợi lớn cho người lao động.

Hãy trả về kết quả dưới dạng ĐÚNG 1 cấu trúc JSON duy nhất (không kèm văn bản giải thích ngoài JSON) theo mẫu:
{
  ""Clauses"": [
    {
      ""ClauseTitle"": ""Tên điều khoản (VD: Thử việc, Tiền lương, Bồi thường)"",
      ""OriginalText"": ""Bắt buộc trích dẫn nguyên văn 100% từng câu từng từ trong hợp đồng gốc, không được tóm tắt hay sửa đổi dấu câu"",
      ""Decision"": ""DongY"",
      ""RiskLevel"": ""Thap"",
      ""LegalReference"": ""Căn cứ pháp lý cụ thể (VD: Khoản 2 Điều 124 Bộ luật Lao động 2019)"",
      ""Strategy"": {
        ""WhyNegotiate"": ""Phân tích sắc bén nguyên nhân gốc rễ vì sao điều khoản này bất lợi, bắt buộc phải chỉ rõ vi phạm hoặc điểm rủi ro đối với quyền lợi của người lao động"",
        ""ProposedText"": ""Cung cấp câu chữ sửa đổi hoàn chỉnh, chuẩn mực pháp lý và cân bằng lợi ích để người lao động đưa trực tiếp cho nhà tuyển dụng"",
        ""TalkingPoints"": ""Kịch bản giao tiếp gồm 2-3 ý ngắn gọn, sắc bén, có trích dẫn luật để ứng viên tự tin thuyết phục nhà tuyển dụng mà không sợ xung đột""
      }
    }
  ]
}
";
                    // 1. Cấu hình Payload cho Groq API (giữ nguyên biến của anh)
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

                    string activeKey = string.IsNullOrWhiteSpace(apiKey) ? GroqApiKey : apiKey;
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

                    // Trích xuất nội dung trả về theo chuẩn OpenAI/Groq (giữ nguyên biến aiText)
                    aiText = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "";

                    var match = System.Text.RegularExpressions.Regex.Match(aiText, @"\{.*\}", System.Text.RegularExpressions.RegexOptions.Singleline);
                    string json = match.Success ? match.Value : aiText;

                    var result = JsonSerializer.Deserialize<ContractAnalysisResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null) return result;
                }
                catch (Exception ex)
                {
                    throw new Exception($"LỖI GỌI GROQ API: {ex.Message} | Nội dung gốc: {aiText}", ex);
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
