using OcrApi.Models;
using Tesseract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace OcrApi.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly string _tessdataPath;

        public TesseractOcrService(ILogger<TesseractOcrService> logger)
        {
            _logger = logger;
            _tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
            
            // Create tessdata directory if it doesn't exist
            if (!Directory.Exists(_tessdataPath))
            {
                Directory.CreateDirectory(_tessdataPath);
            }
        }

        public async Task<string> ExtractTextAsync(byte[] imageData, string language = "tha+eng")
        {
            try
            {
                using var image = Image.Load<Rgba32>(imageData);
                using var engine = new TesseractEngine(_tessdataPath, language, EngineMode.Default);
                
                // Convert ImageSharp image to byte array for Tesseract
                using var ms = new MemoryStream();
                await image.SaveAsPngAsync(ms);
                var pngData = ms.ToArray();
                
                using var img = Pix.LoadFromMemory(pngData);
                using var page = engine.Process(img);
                
                var text = page.GetText();
                var confidence = page.GetMeanConfidence();
                
                _logger.LogInformation("OCR completed with confidence: {Confidence}", confidence);
                
                return await Task.FromResult(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OCR processing");
                throw new InvalidOperationException("Failed to extract text from image", ex);
            }
        }

        public async Task<string> ExtractTextFromPdfAsync(byte[] pdfData, string language = "tha+eng")
        {
            try
            {
                var extractedTexts = new List<string>();

                // For PDF OCR, we would need to convert PDF pages to images first
                // This is a simplified implementation - in production, you'd want to use a library like PDFium
                using var stream = new MemoryStream(pdfData);
                using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                
                // Note: PdfSharp doesn't directly support extracting images from PDFs for OCR
                // This is a placeholder implementation
                // In a real scenario, you'd use libraries like PDFium.NET or convert PDF to images
                
                _logger.LogWarning("PDF OCR not fully implemented - returning placeholder text");
                return await Task.FromResult("PDF OCR processing requires additional image extraction libraries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PDF OCR processing");
                throw new InvalidOperationException("Failed to extract text from PDF", ex);
            }
        }

        public async Task<FinancialDocumentData> ExtractFinancialDataAsync(string extractedText)
        {
            await Task.Delay(10); // Simulate async processing
            
            var financialData = new FinancialDocumentData();

            try
            {
                // Extract document number (pattern: numbers only, typically 6-8 digits)
                var docNumberMatch = Regex.Match(extractedText, @"\b\d{6,8}\b");
                if (docNumberMatch.Success)
                {
                    financialData.DocumentNumber = docNumberMatch.Value;
                }

                // Extract person name (pattern: MRS./MR./MS. followed by name)
                var nameMatch = Regex.Match(extractedText, @"(?:MRS?\.|MS\.)\s+([A-Z\s]+)", RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                {
                    financialData.PersonName = nameMatch.Groups[1].Value.Trim();
                }

                // Extract reference number (pattern: numbers/numbers-numbers-numbers)
                var refMatch = Regex.Match(extractedText, @"\d+/\d{2}-\d{2}-\d{2}");
                if (refMatch.Success)
                {
                    financialData.ReferenceNumber = refMatch.Value;
                }

                // Extract amounts (pattern: decimal numbers, typically with .00)
                var amountMatches = Regex.Matches(extractedText, @"\b\d{1,3}(?:,\d{3})*\.?\d{2}\b");
                var amounts = new List<decimal>();
                
                foreach (Match match in amountMatches)
                {
                    if (decimal.TryParse(match.Value.Replace(",", ""), NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal amount))
                    {
                        amounts.Add(amount);
                    }
                }

                if (amounts.Any())
                {
                    // The largest amount is likely the total
                    financialData.TotalAmount = amounts.Max();
                    
                    // Add expense items for smaller amounts
                    foreach (var amount in amounts.Where(a => a < financialData.TotalAmount))
                    {
                        financialData.ExpenseItems.Add(new ExpenseItem 
                        { 
                            Description = "Extracted expense item",
                            Amount = amount 
                        });
                    }
                }

                // Extract date (pattern: DD-MM-YY or DD/MM/YY)
                var dateMatch = Regex.Match(extractedText, @"\d{2}[-/]\d{2}[-/]\d{2}");
                if (dateMatch.Success && DateTime.TryParseExact(dateMatch.Value, new[] { "dd-MM-yy", "dd/MM/yy", "dd-MM-yyyy", "dd/MM/yyyy" }, 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    financialData.DocumentDate = parsedDate;
                }

                _logger.LogInformation("Extracted financial data: DocumentNumber={DocumentNumber}, PersonName={PersonName}, TotalAmount={TotalAmount}", 
                    financialData.DocumentNumber, financialData.PersonName, financialData.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting financial data");
            }

            return financialData;
        }
    }
}