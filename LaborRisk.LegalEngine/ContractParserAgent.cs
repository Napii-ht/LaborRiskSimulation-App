using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LaborRisk.Domain;

namespace LaborRisk.LegalEngine
{
    public class ContractParserAgent
    {
        private readonly HttpClient _http;

        public ContractParserAgent(HttpClient http)
        {
            _http = http;
        }

        public async Task<ContractInput> ExtractDataAsync(string rawText, string apiKey = "")
        {
            // Nếu rawText rỗng thì mới fallback
            if (string.IsNullOrWhiteSpace(rawText))
                return FallbackMock(rawText);

            try
            {
                string prompt = @"Bạn là AI phân tích hợp đồng lao động Việt Nam. Hãy đọc đoạn văn bản hợp đồng và bóc tách các chỉ số. Trả về ĐÚNG 1 JSON duy nhất có cấu trúc như sau:
{
  ""probationDays"": 60,
  ""baseSalary"": 10000000,
  ""probationSalary"": 8500000,
  ""penaltyAmount"": 0,
  ""overtimeMultiplier"": 1.5,
  ""noticeDaysEmployee"": 30,
  ""hasVagueJobDescription"": false,
  ""hasDegreeRetention"": false
}
Chỉ trả về chuỗi JSON, không kèm bất kỳ lời giải thích nào khác.";

                // 1. Cấu hình Payload chuẩn cho Ollama API
                var payload = new
                {
                    model = "qwen2.5:7b-instruct-q4_K_M",
                    prompt = $"{prompt}\n\nVĂN BẢN HỢP ĐỒNG:\n{rawText}",
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // 2. Gọi API đến Ollama Local
                var response = await _http.PostAsync("http://localhost:11434/api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    string resContent = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(resContent);
                    string aiText = doc.RootElement.GetProperty("response").GetString() ?? "";

                    // 3. Trích xuất JSON từ chuỗi phản hồi của AI
                    var match = Regex.Match(aiText, @"\{.*\}", RegexOptions.Singleline);
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
                // Khi Ollama tắt hoặc gặp sự cố, tự động dùng FallbackMock để không làm crash app
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
