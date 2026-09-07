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

        private string ReadWordFromStream(Stream stream)
        {
            var sb = new StringBuilder();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            using WordprocessingDocument doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body != null)
            {
                foreach (var p in body.Descendants<Paragraph>())
                {
                    sb.AppendLine(p.InnerText);
                }
            }
            return sb.ToString();
        }

        private string ReadPdfFromStream(Stream stream)
        {
            var sb = new StringBuilder();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            using var pdf = PdfDocument.Open(ms);
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private string ReadTxtFromStream(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}