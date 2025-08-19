using Microsoft.AspNetCore.Http;
using OcrApi.Models;
using OcrApi.Services;
using System.Collections.Concurrent;

namespace OcrApi.Services
{
    public interface IProcessingService
    {
        Task<string> ProcessFileAsync(IFormFile file, bool extractFinancialData, string language);
        OcrResult? GetResult(string id);
        IEnumerable<OcrResult> GetAllResults();
    }

    public class ProcessingService : IProcessingService
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<ProcessingService> _logger;
        private readonly ConcurrentDictionary<string, OcrResult> _results = new();

        public ProcessingService(IOcrService ocrService, ILogger<ProcessingService> logger)
        {
            _ocrService = ocrService;
            _logger = logger;
        }

        public async Task<string> ProcessFileAsync(IFormFile file, bool extractFinancialData, string language)
        {
            var result = new OcrResult
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                Status = ProcessingStatus.Processing
            };

            _results[result.Id] = result;

            // Read file data BEFORE starting background task to avoid stream disposal issues
            byte[] fileData;
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {FileName}", file.FileName);
                result.Status = ProcessingStatus.Failed;
                result.Errors.Add($"Failed to read file: {ex.Message}");
                result.ProcessedAt = DateTime.UtcNow;
                return result.Id;
            }

            // Store file type for background processing
            var fileContentType = file.ContentType;

            // Process in background
            _ = Task.Run(async () =>
            {
                try
                {
                    string extractedText;
                    if (fileContentType?.Contains("pdf") == true)
                    {
                        extractedText = await _ocrService.ExtractTextFromPdfAsync(fileData, language);
                    }
                    else
                    {
                        extractedText = await _ocrService.ExtractTextAsync(fileData, language);
                    }

                    result.ExtractedText = extractedText;
                    result.ConfidenceScore = CalculateConfidenceScore(extractedText);

                    if (extractFinancialData && !string.IsNullOrEmpty(extractedText))
                    {
                        result.FinancialData = await _ocrService.ExtractFinancialDataAsync(extractedText);
                    }

                    result.Status = ProcessingStatus.Completed;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {FileName}", result.FileName);
                    result.Status = ProcessingStatus.Failed;
                    result.Errors.Add(ex.Message);
                }
                finally
                {
                    result.ProcessedAt = DateTime.UtcNow;
                }
            });

            return result.Id;
        }

        public OcrResult? GetResult(string id)
        {
            _results.TryGetValue(id, out var result);
            return result;
        }

        public IEnumerable<OcrResult> GetAllResults()
        {
            return _results.Values.OrderByDescending(r => r.ProcessedAt);
        }

        private static double CalculateConfidenceScore(string extractedText)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return 0.0;

            // Simple confidence calculation based on text characteristics
            var lines = extractedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var totalChars = extractedText.Length;
            var alphanumericChars = extractedText.Count(char.IsLetterOrDigit);
            
            return Math.Min(100.0, (alphanumericChars * 100.0 / totalChars) * 0.8 + lines.Length * 2);
        }
    }
}