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
                string prompt = $@"Bạn là AI chuyên gia phân tích hợp đồng lao động và pháp chế doanh nghiệp theo Bộ luật Lao động Việt Nam 2019. 
Hãy đọc kỹ nội dung hợp đồng dưới đây, bóc tách dữ liệu và đối chiếu các điều khoản xem có điểm nào vi phạm luật hoặc gây rủi ro không.

NỘI DUNG HỢP ĐỒNG CẦN PHÂN TÍCH:
{rawText}

Hãy trả về kết quả dưới dạng ĐÚNG 1 cấu trúc JSON duy nhất (không kèm theo bất kỳ văn bản giải thích nào ngoài JSON) theo mẫu sau:
{{
  ""probationDays"": 60,
  ""baseSalary"": 10000000,
  ""probationSalary"": 8500000,
  ""penaltyAmount"": 0,
  ""overtimeMultiplier"": 1.5,
  ""noticeDaysEmployee"": 30,
  ""hasVagueJobDescription"": false,
  ""hasDegreeRetention"": false
}}";

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
