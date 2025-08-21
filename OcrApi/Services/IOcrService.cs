using OcrApi.Models;

namespace OcrApi.Services
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(byte[] imageData, string language = "tha+eng");
        Task<string> ExtractTextFromPdfAsync(byte[] pdfData, string language = "tha+eng");
        Task<FinancialDocumentData> ExtractFinancialDataAsync(string extractedText);
        Task<Dictionary<string, object>> ExtractDynamicDataAsync(string extractedText, string templateName = "auto");
        object FormatOcrTextAsReadableObject(string extractedText);
    }
}