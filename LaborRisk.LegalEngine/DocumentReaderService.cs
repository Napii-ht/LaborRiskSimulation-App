using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace LaborRisk.LegalEngine
{
    public class DocumentReaderService
    {
        public string ExtractTextFromStream(Stream stream, string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".docx" => ReadWordFromStream(stream),
                ".pdf" => ReadPdfFromStream(stream),
                ".txt" => ReadTxtFromStream(stream),
                _ => throw new NotSupportedException("Chỉ hỗ trợ file .docx, .pdf hoặc .txt")
            };
        }
        public string HighlightRiskyClauses(string fullContractText, List<string> riskySnippets)
        {
            if (string.IsNullOrEmpty(fullContractText) || riskySnippets == null || !riskySnippets.Any())
                return fullContractText;

            string highlightedText = fullContractText.Replace("\r", "");

            foreach (var snippet in riskySnippets)
            {
                if (string.IsNullOrWhiteSpace(snippet))
                    continue;
                string pattern = System.Text.RegularExpressions.Regex.Escape(snippet.Trim());
                pattern = System.Text.RegularExpressions.Regex.Replace(pattern, @"\s+", @"\s*");
                highlightedText = System.Text.RegularExpressions.Regex.Replace(highlightedText, pattern, "<mark class='bg-warning text-dark p-1 rounded'>$0</mark>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            }

            return highlightedText.Replace("\n", "<br />");
        }
        private string ReadWordFromStream(Stream stream)
        {
            var sb = new StringBuilder();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            using WordprocessingDocument doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body != null)
            {
                foreach (var p in body.Descendants<Paragraph>())
                {
                    string text = p.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }
            string rawText = sb.ToString();
            rawText = System.Text.RegularExpressions.Regex.Replace(rawText, @"[ \t]+", " ");
            return rawText.Trim();
        }

        private string ReadPdfFromStream(Stream stream)
        {
            var sb = new StringBuilder();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            using var pdf = PdfDocument.Open(ms);
            foreach (var page in pdf.GetPages())
            {
                string pageText = page.Text?.Trim();
                if (!string.IsNullOrEmpty(pageText))
                {
                    sb.AppendLine(pageText);
                }
            }
            string rawText = sb.ToString();
            rawText = System.Text.RegularExpressions.Regex.Replace(rawText, @"[ \t\r\n]+", " ");
            return rawText.Trim();
        }

        private string ReadTxtFromStream(Stream stream)
        {
            if (stream.CanSeek) stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}