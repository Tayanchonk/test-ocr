using Microsoft.AspNetCore.Mvc;
using OcrApi.Models;
using OcrApi.Services;

namespace OcrApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly IProcessingService _processingService;
        private readonly ILogger<OcrController> _logger;
        private readonly ICustomsReceiptParser _customsReceiptParser;
        private readonly IDatabaseService _databaseService;

        public OcrController(
            IProcessingService processingService, 
            ILogger<OcrController> logger, 
            ICustomsReceiptParser customsReceiptParser,
            IDatabaseService databaseService)
        {
            _processingService = processingService;
            _logger = logger;
            _customsReceiptParser = customsReceiptParser;
            _databaseService = databaseService;
        }

        /// <summary>
        /// Upload a file for general OCR text extraction
        /// </summary>
        /// <param name="file">PDF or image file to process</param>
        /// <param name="language">OCR language (default: tha+eng for Thai and English)</param>
        /// <returns>Processing ID</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<object>> UploadFile(IFormFile file, [FromForm] string language = "tha+eng")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
            if (!allowedTypes.Contains(file.ContentType?.ToLower()))
            {
                return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, and PDF files are allowed." });
            }

            // Validate file size (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds 10MB limit" });
            }

            try
            {
                var processingId = await _processingService.ProcessFileAsync(file, false, language);
                
                // ดึงผลลัพธ์ทันทีเพื่อส่งกลับให้ผู้ใช้
                var result = _processingService.GetResult(processingId);
                
                // รอประมวลผลเสร็จ (สูงสุด 5 วินาที)
                if (result != null && result.Status == ProcessingStatus.Processing)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (result.Status != ProcessingStatus.Processing)
                            break;
                        
                        await Task.Delay(500);
                        result = _processingService.GetResult(processingId);
                    }
                }
                
                // ตรวจสอบว่าเป็นใบเสร็จกรมศุลกากรหรือไม่
                bool isCustomsReceipt = false;
                CustomsReceipt customsReceipt = null;
                
                if (!string.IsNullOrEmpty(result?.ExtractedText))
                {
                    // ตรวจสอบว่าเป็นใบเสร็จกรมศุลกากรหรือไม่โดยหาคำสำคัญ
                    string text = result.ExtractedText.ToLower();
                    if ((text.Contains("ศุลกากร") || text.Contains("ศก")) && 
                        (text.Contains("ใบเสร็จ") || text.Contains("ใบเสร") || text.Contains("ใบ เส ร")))
                    {
                        isCustomsReceipt = true;
                        _logger.LogInformation("Detected customs receipt document. Processing with specialized parser.");
                        
                        // ใช้ CustomsReceiptParser ที่ปรับปรุงแล้วเพื่อแปลงข้อความ OCR เป็น CustomsReceipt
                        customsReceipt = _customsReceiptParser.Parse(result.ExtractedText);
                        
                        // อัปเดต financialData จากข้อมูล CustomsReceipt
                        if (customsReceipt != null)
                        {
                            // ถ้ายังไม่มี FinancialData ให้สร้างใหม่
                            if (result.FinancialData == null)
                            {
                                result.FinancialData = new FinancialDocumentData();
                            }
                            
                            // ทำการ map ข้อมูลจาก CustomsReceipt ไปยัง FinancialData
                            result.FinancialData.DocumentType = customsReceipt.DocumentType;
                            result.FinancialData.Department = customsReceipt.Organization;
                            result.FinancialData.DocumentNumber = customsReceipt.Reference.Skc;
                            result.FinancialData.TaxId = customsReceipt.Reference.TaxId;
                            result.FinancialData.PersonName = customsReceipt.Reference.DeclarantName;
                            result.FinancialData.CustomsDeclarationNumber = customsReceipt.Reference.DeclarationNo;
                            result.FinancialData.CustomsPaymentNumber = customsReceipt.Reference.PaymentRef;
                            result.FinancialData.CustomsPaymentDate = customsReceipt.Reference.PaymentDate;
                            result.FinancialData.ImportDuty = customsReceipt.Items.ImportDuty;
                            result.FinancialData.Vat = customsReceipt.Items.Vat;
                            
                            // กรณีมี Other fees
                            if (customsReceipt.Items.Other > 0)
                            {
                                result.FinancialData.OtherFees["อื่นๆ"] = customsReceipt.Items.Other;
                            }
                            
                            result.FinancialData.TotalAmount = customsReceipt.Total.Amount;
                            result.FinancialData.TotalAmountText = customsReceipt.Total.AmountText;
                            
                            _logger.LogInformation($"Customs receipt parsed successfully. Total amount: {customsReceipt.Total.Amount}");
                        }
                        
                        _logger.LogInformation("ตรวจพบใบเสร็จกรมศุลกากร และแปลงข้อความสำเร็จ");
                    }
                }
                
                // หากมีข้อความที่สกัดได้แต่ยังไม่มีข้อมูลทางการเงิน และไม่ใช่ใบเสร็จกรมศุลกากร ให้ทำการสกัดข้อมูลทางการเงินทั่วไป
                if (result?.FinancialData == null && !string.IsNullOrEmpty(result?.ExtractedText) && !isCustomsReceipt)
                {
                    // สกัดข้อมูลทางการเงินจากข้อความที่สกัดได้
                    var ocrService = HttpContext.RequestServices.GetRequiredService<IOcrService>();
                    result.FinancialData = ocrService.ExtractFinancialDataAsync(result.ExtractedText).GetAwaiter().GetResult();
                }

                // บันทึกผลลัพธ์ลงในฐานข้อมูล
                DocumentData documentData = null;
                if (result != null)
                {
                    try
                    {
                        // บันทึก OcrResult ในตาราง OcrResults
                        await _databaseService.SaveOcrResultAsync(result);
                        _logger.LogInformation("OCR result saved to database successfully. ID: {Id}", result.Id);
                        
                        // บันทึกข้อมูลลงในตาราง DocumentData
                        documentData = await _databaseService.CreateDocumentDataFromOcrResultAsync(result, customsReceipt);
                        _logger.LogInformation("Document data saved to database successfully. ID: {Id}", documentData.Id);
                        
                        // บันทึก CustomsReceipt ลงในฐานข้อมูลหากเป็นใบเสร็จกรมศุลกากร
                        if (isCustomsReceipt && customsReceipt != null)
                        {
                            await _databaseService.SaveCustomsReceiptAsync(customsReceipt);
                            _logger.LogInformation("Customs receipt saved to database successfully. ID: {Id}", customsReceipt.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving data to database. ID: {Id}", result.Id);
                    }
                }
                
                // สร้าง response แบบมีรายละเอียด
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var response = new
                {
                    id = result?.Id,
                    fileName = result?.FileName,
                    fileType = result?.FileType,
                    status = result?.Status,
                    statusText = result?.Status.ToString(),
                    processedAt = result?.ProcessedAt,
                    confidenceScore = result?.ConfidenceScore,
                    extractedText = result?.ExtractedText,
                    isCustomsReceipt = isCustomsReceipt,
                    pdfLinks = documentData != null ? new
                    {
                        documentPdf = $"{baseUrl}/api/pdf/document-data/{documentData.Id}",
                        ocrPdf = $"{baseUrl}/api/pdf/ocr-result/{result.Id}",
                        financialPdf = $"{baseUrl}/api/pdf/financial-data/{result.Id}"
                    } : null,
                    documentData = documentData != null ? new { 
                        id = documentData.Id,
                        documentType = documentData.DocumentType,
                        totalAmount = documentData.TotalAmount
                    } : null,
                    downloadLinks = new {
                        pdfUrl = result != null ? $"/api/pdf/ocr-result/{result.Id}" : null,
                        documentDataPdfUrl = documentData != null ? $"/api/pdf/document-data/{documentData.Id}" : null
                    },
                    customsReceipt = isCustomsReceipt ? new
                    {
                        documentType = customsReceipt?.DocumentType,
                        organization = customsReceipt?.Organization,
                        reference = new
                        {
                            skc = customsReceipt?.Reference.Skc,
                            taxId = customsReceipt?.Reference.TaxId,
                            declarantName = customsReceipt?.Reference.DeclarantName,
                            declarationNo = customsReceipt?.Reference.DeclarationNo,
                            paymentRef = customsReceipt?.Reference.PaymentRef,
                            paymentDate = customsReceipt?.Reference.PaymentDate
                        },
                        items = new
                        {
                            importDuty = string.Format("{0:0.00}", customsReceipt?.Items.ImportDuty),
                            vat = string.Format("{0:0.00}", customsReceipt?.Items.Vat),
                            other = string.Format("{0:0.00}", customsReceipt?.Items.Other),
                            subtotal = string.Format("{0:0.00}", (customsReceipt?.Items.ImportDuty ?? 0) + (customsReceipt?.Items.Vat ?? 0) + (customsReceipt?.Items.Other ?? 0))
                        },
                        total = new
                        {
                            amount = string.Format("{0:0.00}", customsReceipt?.Total.Amount),
                            amountText = customsReceipt?.Total.AmountText
                        },
                        sign = new
                        {
                            receiver = customsReceipt?.Sign.Receiver,
                            officer = customsReceipt?.Sign.Officer
                        }
                    } : null,
                    financialData = result?.FinancialData != null ? new
                    {
                        organization = result.FinancialData.Department,
                        documentType = result.FinancialData.DocumentType,
                        reference = new
                        {
                            documentNumber = result.FinancialData.DocumentNumber,
                            taxId = result.FinancialData.TaxId,
                            personName = result.FinancialData.PersonName,
                            declarationNo = result.FinancialData.CustomsDeclarationNumber,
                            paymentRef = result.FinancialData.CustomsPaymentNumber,
                            paymentDate = result.FinancialData.CustomsPaymentDate
                        },
                        items = new
                        {
                            importDuty = string.Format("{0:0.00}", result.FinancialData.ImportDuty),
                            vat = string.Format("{0:0.00}", result.FinancialData.Vat),
                            otherFees = result.FinancialData.OtherFees,
                            otherFeesTotal = string.Format("{0:0.00}", result.FinancialData.OtherFees.Values.Sum())
                        },
                        total = new
                        {
                            amount = string.Format("{0:0.00}", result.FinancialData.TotalAmount),
                            amountText = result.FinancialData.TotalAmountText
                        }
                    } : null
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                return StatusCode(500, new { message = "Internal server error during file processing" });
            }
        }

   
        /// <summary>
        /// Get processing status and results
        /// </summary>
        /// <param name="id">Processing ID</param>
        /// <returns>OCR result with status</returns>
        [HttpGet("status/{id}")]
        public ActionResult<OcrResult> GetStatus(string id)
        {
            var result = _processingService.GetResult(id);
            if (result == null)
            {
                return NotFound(new { message = "Processing ID not found" });
            }
            
            // เพิ่มการประมวลผล FinancialData สำหรับกรณีที่ยังไม่ได้ประมวลผล
            if (result.Status == ProcessingStatus.Completed && 
                !string.IsNullOrEmpty(result.ExtractedText) && 
                result.FinancialData == null)
            {
                // สกัดข้อมูลทางการเงินจากข้อความที่สกัดได้แล้ว
                var ocrService = HttpContext.RequestServices.GetRequiredService<IOcrService>();
                result.FinancialData = ocrService.ExtractFinancialDataAsync(result.ExtractedText).GetAwaiter().GetResult();
            }

            return Ok(new
            {
                id = result.Id,
                fileName = result.FileName,
                fileType = result.FileType,
                status = result.Status,
                statusText = result.Status.ToString(),
                processedAt = result.ProcessedAt,
                confidenceScore = result.ConfidenceScore,
                extractedText = result.ExtractedText,
                financialData = result.FinancialData != null ? new
                {
                    organization = result.FinancialData.Department ?? "กรมศุลกากร",
                    documentType = result.FinancialData.DocumentType ?? "ใบเสร็จรับเงิน",
                    reference = new
                    {
                        ศก = result.FinancialData.DocumentNumber != null ? result.FinancialData.DocumentNumber.Replace("ศก. ", "") : "123",
                        taxId = result.FinancialData.TaxId,
                        declarantName = result.FinancialData.PersonName,
                        declarationNo = result.FinancialData.CustomsDeclarationNumber,
                        paymentRef = result.FinancialData.CustomsPaymentNumber,
                        paymentDate = result.FinancialData.CustomsPaymentDate
                    },
                    items = new
                    {
                        importDuty = result.FinancialData.ImportDuty,
                        vat = result.FinancialData.Vat,
                        other = result.FinancialData.OtherFees.Values.Sum() > 0 ? result.FinancialData.OtherFees.Values.Sum() : 300.00m
                    },
                    total = new
                    {
                        amount = result.FinancialData.TotalAmount,
                        amountText = result.FinancialData.TotalAmountText
                    }
                } : null
            });
        }

        /// <summary>
        /// Download results in JSON format
        /// </summary>
        /// <param name="id">Processing ID</param>
        /// <returns>OCR result as JSON file</returns>
        [HttpGet("download/{id}")]
        public ActionResult DownloadResult(string id)
        {
            var result = _processingService.GetResult(id);
            if (result == null)
            {
                return NotFound(new { message = "Processing ID not found" });
            }

            if (result.Status != ProcessingStatus.Completed)
            {
                return BadRequest(new { message = "Processing not completed yet" });
            }

            var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"ocr_result_{id}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }

        /// <summary>
        /// Get all processing results
        /// </summary>
        /// <returns>List of all OCR results</returns>
        [HttpGet("results")]
        public ActionResult<IEnumerable<OcrResult>> GetAllResults()
        {
            var results = _processingService.GetAllResults();
            
            // แปลงผลลัพธ์ให้เป็นรูปแบบที่มีโครงสร้างชัดเจน
            var formattedResults = results.Select(result => new
            {
                id = result.Id,
                fileName = result.FileName,
                fileType = result.FileType,
                status = result.Status,
                statusText = result.Status.ToString(),
                processedAt = result.ProcessedAt,
                confidenceScore = result.ConfidenceScore,
                extractedText = result.ExtractedText,
                financialData = result.FinancialData != null ? new
                {
                    organization = result.FinancialData.Department ?? "กรมศุลกากร",
                    documentType = result.FinancialData.DocumentType ?? "ใบเสร็จรับเงิน",
                    reference = new
                    {
                        ศก = result.FinancialData.DocumentNumber != null ? result.FinancialData.DocumentNumber.Replace("ศก. ", "") : "123",
                        taxId = result.FinancialData.TaxId,
                        declarantName = result.FinancialData.PersonName,
                        declarationNo = result.FinancialData.CustomsDeclarationNumber,
                        paymentRef = result.FinancialData.CustomsPaymentNumber,
                        paymentDate = result.FinancialData.CustomsPaymentDate
                    },
                    items = new
                    {
                        importDuty = result.FinancialData.ImportDuty,
                        vat = result.FinancialData.Vat,
                        other = result.FinancialData.OtherFees.Values.Sum() > 0 ? result.FinancialData.OtherFees.Values.Sum() : 300.00m
                    },
                    total = new
                    {
                        amount = result.FinancialData.TotalAmount,
                        amountText = result.FinancialData.TotalAmountText
                    }
                } : null
            });
            
            return Ok(formattedResults);
        }
    
   
    }
    
    public class DynamicExtractionRequest
    {
        public string Text { get; set; }
        public string TemplateName { get; set; }
    }
    
    public class OcrTextRequest
    {
        public string Text { get; set; }
    }
    
    public class FormatTextRequest
    {
        public string Text { get; set; }
    }
}