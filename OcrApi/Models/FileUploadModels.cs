namespace OcrApi.Models
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public bool ExtractFinancialData { get; set; } = false;
        public string? Language { get; set; } = "tha+eng"; // Thai + English for Tesseract
    }

    public class FileUploadResponse
    {
        public string Id { get; set; } = null!;
        public string Message { get; set; } = null!;
        public ProcessingStatus Status { get; set; }
    }
}