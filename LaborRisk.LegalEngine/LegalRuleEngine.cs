using System;
using LaborRisk.Domain;

namespace LaborRisk.LegalEngine
{
    public class LegalRuleEngine
    {
        public AssessmentResult EvaluateContract(ContractInput contract)
        {
            var result = new AssessmentResult();
            int totalPenalty = 0;

            // NHÓM 1: Thời gian và lương thử việc
            if (contract.ProbationDays > 60)
            {
                var risk = new RiskItem { Category = "Nhóm 1: Thử việc", Title = "Thử việc vượt quá 60 ngày", Description = $"Thời gian thử việc {contract.ProbationDays} ngày là trái luật.", LawReference = "Điều 25 BLLĐ 2019", DeductedScore = 20 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }
            if (contract.BaseSalary > 0 && (double)contract.ProbationSalary / contract.BaseSalary < 0.85)
            {
                var risk = new RiskItem { Category = "Nhóm 1: Thử việc", Title = "Lương thử việc dưới 85%", Description = "Mức lương thử việc không đạt 85% lương chính thức.", LawReference = "Điều 26 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 2: Phạt vi phạm và bồi thường chi phí đào tạo
            if (contract.PenaltyAmount > 0)
            {
                var risk = new RiskItem { Category = "Nhóm 2: Phạt & Đào tạo", Title = "Phạt tiền người lao động", Description = $"Quy định phạt {contract.PenaltyAmount:N0} VNĐ hoặc trừ lương là hành vi bị cấm.", LawReference = "Khoản 2 Điều 127 BLLĐ 2019", DeductedScore = 25 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }
            if (contract.HasDegreeRetention)
            {
                var risk = new RiskItem { Category = "Nhóm 2: Phạt & Đào tạo", Title = "Giữ giấy tờ/Bằng cấp gốc", Description = "Không được giữ bản chính giấy tờ tùy thân, văn bằng của người lao động.", LawReference = "Khoản 2 Điều 17 BLLĐ 2019", DeductedScore = 25 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 3: Cam kết bảo mật, không cạnh tranh (NDA & Non-compete)
            if (contract.HasVagueJobDescription) // Tạm map hoặc thay thế bằng biến check Non-compete/NDA độc hại
            {
                var risk = new RiskItem { Category = "Nhóm 3: Bảo mật & Cạnh tranh", Title = "Cam kết hạn chế cạnh tranh bất hợp lý", Description = "Thỏa thuận hạn chế việc làm sau khi nghỉ việc nhưng không kèm hỗ trợ sinh phí.", LawReference = "Điều 23 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 4: Thời giờ làm việc và làm thêm giờ
            if (contract.OvertimeMultiplier > 0 && contract.OvertimeMultiplier < 1.5)
            {
                var risk = new RiskItem { Category = "Nhóm 4: Thời giờ làm việc", Title = "Trả lương OT thiếu", Description = $"Hệ số tăng ca {contract.OvertimeMultiplier}x thấp hơn quy định tối thiểu 1.5x.", LawReference = "Điều 98 & Điều 107 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 5: Điều kiện đơn phương chấm dứt hợp đồng
            if (contract.NoticeDaysEmployee > 45)
            {
                var risk = new RiskItem { Category = "Nhóm 5: Chấm dứt HĐ", Title = "Thời gian báo trước quá dài", Description = $"Bắt buộc người lao động báo trước {contract.NoticeDaysEmployee} ngày vượt quá thời hạn luật định.", LawReference = "Điều 35 BLLĐ 2019", DeductedScore = 10 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            result.LriScore = Math.Max(0, 100 - totalPenalty);
            return result;
        }
    }
}