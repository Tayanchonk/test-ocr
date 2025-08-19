namespace OcrApi.Models
{
    public class OcrResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Processing;
        public string? ExtractedText { get; set; }
        public FinancialDocumentData? FinancialData { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public List<string> Errors { get; set; } = new();
        public double ConfidenceScore { get; set; }
    }

    public enum ProcessingStatus
    {
        Processing,
        Completed,
        Failed
    }
}