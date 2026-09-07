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

            // NHÓM 1: Thử việc & Lương thử việc
            if (contract.ProbationDays > 60)
            {
                var risk = new RiskItem { Category = "Nhóm 1", Title = "Thử việc vượt quá 60 ngày", Description = $"Thời gian thử việc {contract.ProbationDays} ngày là trái luật.", LawReference = "Điều 25 BLLĐ 2019", DeductedScore = 20 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }
            if (contract.BaseSalary > 0 && (double)contract.ProbationSalary / contract.BaseSalary < 0.85)
            {
                var risk = new RiskItem { Category = "Nhóm 1", Title = "Lương thử việc dưới 85%", Description = "Mức lương thử việc không đạt 85% lương chính thức.", LawReference = "Điều 26 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 2: Phạt tiền & Giữ bằng gốc
            if (contract.PenaltyAmount > 0)
            {
                var risk = new RiskItem { Category = "Nhóm 2", Title = "Phạt tiền người lao động", Description = $"Quy định phạt {contract.PenaltyAmount:N0} VNĐ là hành vi bị cấm.", LawReference = "Điều 127 BLLĐ 2019", DeductedScore = 25 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }
            if (contract.HasDegreeRetention)
            {
                var risk = new RiskItem { Category = "Nhóm 2", Title = "Giữ giấy tờ/Bằng cấp gốc", Description = "Không được giữ bản chính giấy tờ tùy thân, văn bằng.", LawReference = "Điều 17 BLLĐ 2019", DeductedScore = 25 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 3: Mô tả mập mờ
            if (contract.HasVagueJobDescription)
            {
                var risk = new RiskItem { Category = "Nhóm 3", Title = "Mô tả công việc mập mờ", Description = "Ép làm việc ngoài phạm vi thỏa thuận không phụ cấp.", LawReference = "Điều 21 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 4: Lương OT
            if (contract.OvertimeMultiplier > 0 && contract.OvertimeMultiplier < 1.5)
            {
                var risk = new RiskItem { Category = "Nhóm 4", Title = "Trả lương OT thiếu", Description = $"Hệ số {contract.OvertimeMultiplier}x thấp hơn quy định tối thiểu 1.5x.", LawReference = "Điều 98 BLLĐ 2019", DeductedScore = 15 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            // NHÓM 5: Thời gian báo trước
            if (contract.NoticeDaysEmployee > 45)
            {
                var risk = new RiskItem { Category = "Nhóm 5", Title = "Thời gian báo trước quá dài", Description = $"Bắt buộc báo trước {contract.NoticeDaysEmployee} ngày vượt hạn định luật.", LawReference = "Điều 35 BLLĐ 2019", DeductedScore = 10 };
                result.Risks.Add(risk); totalPenalty += risk.DeductedScore;
            }

            result.LriScore = Math.Max(0, 100 - totalPenalty);
            return result;
        }
    }
}