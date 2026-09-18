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
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = _configuration["GroqApiKey"];
                apiKey = _configuration["GroqApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            }
            if (string.IsNullOrWhiteSpace(rawText))
                return FallbackMock(rawText);
            string aiText = "";
            try
            {
                string prompt = @"
Bạn là Chuyên gia Pháp lý và Cố vấn Đàm phán Hợp đồng Lao động theo Bộ luật Lao động Việt Nam 2019.
Nhiệm vụ của bạn là phân tích hợp đồng được cung cấp và đưa ra cố vấn chiến lược mang tính thực chiến cao cho từng điều khoản.

VỚI MỖI ĐIỀU KHOẢN, HÃY XÁC ĐỊNH 'decision' THEO 3 HƯỚNG:
1. 'DongY': Điều khoản chuẩn xác, công bằng, tuân thủ pháp luật.
2. 'TuChoi': Điều khoản vi phạm điều cấm của pháp luật nghiêm trọng (như phạt tiền, giữ giấy tờ gốc), không thể thỏa thuận.
3. 'DamPhan': Điều khoản KHÔNG sai luật hoàn toàn, nhưng chứa rủi ro, mập mờ hoặc gây bất lợi lớn cho người lao động.

Yêu cầu quét bắt buộc:
- BẮT BUỘC trích xuất TẤT CẢ các điều khoản rủi ro/vi phạm có trong hợp đồng (Thử việc, Lương, OT, Nghỉ việc, Phạt vi phạm...). KHÔNG ĐƯỢC bỏ sót bất kỳ điều khoản nào.
- 'originalText': TRÍCH NGUYÊN VĂN chính xác 100% câu chứa rủi ro từ hợp đồng gốc để hệ thống tô đậm.
- 'talkingPoints': ĐÓNG VAI LÀ ỨNG VIÊN NÓI CHUYỆN TRỰC TIẾP VỚI SẾP/HR (xưng 'em', gọi 'anh/chị').

QUY TẮC BẮT BUỘC CHO 'originalText':
- BẮT BUỘC phải TRÍCH NGUYÊN VĂN chính xác 100% câu chứa rủi ro từ hợp đồng gốc.
- TUYỆT ĐỐI KHÔNG tự viết lại, KHÔNG tóm tắt, KHÔNG thêm bớt ký tự để hệ thống tô đậm được trên UI.

Quy tắc cho 'talkingPoints': Đóng vai là ứng viên nói chuyện trực tiếp với sếp/HR (xưng 'em', gọi 'anh/chị'). 
- KHÔNG viết kiểu hướng dẫn (""Nhấn mạnh quyền..."", ""Đề nghị sửa..."").
- PHẢI viết nguyên văn câu thoại nói ra miệng (VD: """"Dạ anh/chị, ở phần làm thêm giờ em thấy theo Luật Lao động thì tiền OT sẽ tính riêng. Mình có thể điều chỉnh lại khoản này để đúng quy định được không?"""")

Quy tắc căn cứ pháp lý bắt buộc (Bộ Luật lao động 2019):
NHÓM 1: THỜI GIAN VÀ LƯƠNG THỬ VIỆC
- Thời gian thử việc tối đa 60 ngày (trình độ cao đẳng trở lên): Khoản 2 Điều 25.
- Thời gian thử việc tối đa 30 ngày (trình độ trung cấp, công nhân): Khoản 3 Điều 25.
- Lương thử việc tối thiểu 85% lương chính thức: Điều 26.
- Cấm thử việc đối với HĐLĐ dưới 01 tháng: Khoản 3 Điều 24.

NHÓM 2: PHẠT VI PHẠM VÀ BỒI THƯỜNG CHI PHÍ ĐÀO TẠO
- KHÔNG ĐƯỢC PHẠT TIỀN / CẮT LƯƠNG THAY XỬ LÝ KỶ LUẬT: Người sử dụng lao động không được dùng hình thức phạt tiền, cắt lương thay việc xử lý kỷ luật lao động (Khoản 2 Điều 127).
- Cấm giữ bản chính giấy tờ tùy thân, văn bằng, chứng chỉ: Khoản 1 Điều 17.
- Cấm yêu cầu đặt cọc tiền/tài sản: Khoản 2 Điều 17.
- Chi phí đào tạo (Khoản 3 Điều 62): Chi phí đào tạo bao gồm các khoản chi có chứng từ hợp lệ theo quy định tại Khoản 3 Điều 62, bao gồm các khoản được luật liệt kê.
- ĐÁNH GIÁ và ĐÀM PHÁN CHI PHÍ ĐÀO TẠO:
  + Nếu hợp đồng yêu cầu hoàn trả các khoản không chứng minh được là chi phí đào tạo hợp lệ -> Đánh giá 'DamPhan'.
  + Nếu hợp đồng quy định mức bồi hoàn/phạt cao hơn chi phí đào tạo thực tế -> Đánh giá 'DamPhan' (hoặc 'CanXemXet'), yêu cầu doanh nghiệp giải trình căn cứ pháp lý và cơ sở tính toán; KHÔNG tự động 'TuChoi' chỉ dựa trên Khoản 3 Điều 62.
  + Hướng xử lý: Yêu cầu doanh nghiệp giải trình căn cứ tính toán và cung cấp đầy đủ chứng từ hợp lệ.
  + Lưu ý: Đề xuất giảm mức hoàn trả theo thời gian đã làm việc là đề xuất đàm phán, KHÔNG PHẢI công thức bắt buộc tại Khoản 3 Điều 62.

NHÓM 3: CAM KẾT BẢO MẬT (NDA) VÀ KHÔNG CẠNH TRANH (NCA)
- Thỏa thuận bảo vệ bí mật kinh doanh, bí mật công nghệ: Khoản 2 Điều 21.
- ĐÁNH GIÁ và ĐÀM PHÁN NCA (Sau khi nghỉ việc):
  + BLLĐ 2019 KHÔNG quy định một thời hạn NCA bắt buộc như 6–12 tháng và KHÔNG quy định bắt buộc phải trả tiền đền bù hàng tháng. Không tự động kết luận NCA không có tiền đền bù là vi phạm pháp luật.
  + Nếu NCA có phạm vi địa lý/lĩnh vực/đối tượng quá rộng hoặc thời hạn hạn chế đáng kể khả năng lựa chọn việc làm -> Đánh giá 'DamPhan' (hoặc 'CanXemXet'), KHÔNG tự động kết luận 'TuChoi'.
  + Đề xuất đàm phán (Thỏa thuận hai bên, không nêu là quy định luật): Đề xuất thu hẹp phạm vi, xác định rõ đối tượng cạnh tranh, giới hạn thời gian hợp lý (ví dụ 6–12 tháng) và có thể thỏa thuận khoản hỗ trợ tài chính trong thời gian bị hạn chế.

NHÓM 4: THỜI GIỜ LÀM VIỆC, LÀM THÊM GIỜ (OT)
- Thời giờ làm việc bình thường (tối đa 8 giờ/ngày, 48 giờ/tuần): Khoản 1 Điều 105.
- Tiền lương làm thêm giờ (Ngày thường >=150%, Nghỉ hàng tuần >=200%, Nghỉ lễ/Tết >=300%): Điểm a, b, c Khoản 1 Điều 98.
- Giới hạn giờ làm thêm (tối đa 40 giờ/tháng, 200 giờ/năm): Điểm b, c Khoản 2 Điều 107.

NHÓM 5: ĐIỀU KIỆN ĐƠN PHƯƠNG CHẤM DỨT HỢP ĐỒNG
- NLĐ đơn phương chấm dứt (Báo trước 45 ngày HĐ vô thời hạn; 30 ngày HĐ 12-36 tháng; 3 ngày HĐ <12 tháng): Điểm a, b, c Khoản 1 Điều 35.
- NLĐ đơn phương chấm dứt KHÔNG cần báo trước (khi bị chậm lương, ngược đãi, vi phạm điều kiện làm việc): Khoản 2 Điều 35.
- NSDLĐ đơn phương chấm dứt (phải thuộc các trường hợp luật định và tuân thủ thời hạn báo trước): Khoản 1 và Khoản 2 Điều 36.

YÊU CẦU PHÂN TÍCH:
- Quét và trích xuất TẤT CẢ các điều khoản rủi ro có trong hợp đồng (không giới hạn số lượng).
- Các trường trong 'strategy' cần viết súc tích, ngắn gọn (tối đa 2 câu) để tập trung vào bản chất pháp lý.

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
        ""talkingPoints"": ""Lời khuyên ngắn gọn để nói chuyện trực tiếp với HR/Sếp nhưng vẫn giữ thái độ đúng mực để được chấp nhận lời đàm phán""
      }
    }
  ]
}";
                
                // 1. Cấu hình Payload cho Groq API (giữ nguyên biến của anh)
                var payload = new
                {
                    model = "groq/compound-mini",
                    messages = new[]
        {
        new { role = "system", content = prompt },
        new { role = "user", content = $"VĂN BẢN HỢP ĐỒNG:\n{rawText}" }
    },
                    response_format = new { type = "json_object" },
                    temperature = 0.2,
                    max_tokens = 4096
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

                string activeKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : _configuration["GroqApiKey"]; request.Headers.Add("Authorization", $"Bearer {activeKey}");
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

                string json = aiText.Replace("```json", "").Replace("```", "").Trim();

                int firstBrace = json.IndexOf('{');
                if (firstBrace >= 0)
                {
                    json = json.Substring(firstBrace);
                }
                else
                {
                    throw new Exception($"Không tìm thấy ký tự mở đầu JSON '{{' trong phản hồi của AI: {aiText}");
                }

                if (!json.EndsWith("}"))
                {
                    int lastBrace = json.LastIndexOf('}');
                    if (lastBrace > 0)
                    {
                        json = json.Substring(0, lastBrace + 1) + "]}";
                    }
                    else
                    {
                        json += "}]}";
                    }
                }
                var result = JsonSerializer.Deserialize<ContractAnalysisResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null && result.Clauses != null && result.Clauses.Any())
                    return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"[LỖI GROQ API]: {ex.Message}");
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
