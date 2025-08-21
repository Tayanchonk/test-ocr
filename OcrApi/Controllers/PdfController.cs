using Microsoft.AspNetCore.Mvc;
using OcrApi.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OcrApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(
            IPdfService pdfService, 
            IDatabaseService databaseService, 
            ILogger<PdfController> logger)
        {
            _pdfService = pdfService;
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("ocr-result/{id}")]
        public async Task<IActionResult> GetPdfFromOcrResult(string id)
        {
            try
            {
                // ตรวจสอบว่ามีผลลัพธ์ OCR หรือไม่
                var ocrResult = await _databaseService.GetOcrResultByIdAsync(id);
                if (ocrResult == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูล OCR ที่มี ID: {id}" });
                }

                // สร้าง PDF
                string fileName = await _pdfService.GeneratePdfFromOcrResult(id);
                if (string.IsNullOrEmpty(fileName))
                {
                    return StatusCode(500, new { message = "ไม่สามารถสร้างไฟล์ PDF ได้" });
                }

                // ส่งไฟล์กลับ
                string filePath = _pdfService.GetPdfFilePath(fileName);
                var fileStream = System.IO.File.OpenRead(filePath);
                return File(fileStream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for OCR result: {Id}", id);
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการสร้างไฟล์ PDF: {ex.Message}" });
            }
        }

        [HttpGet("document-data/{id}")]
        public async Task<IActionResult> GetPdfFromDocumentData(int id)
        {
            try
            {
                // ตรวจสอบว่ามีข้อมูลเอกสารหรือไม่
                var documentData = await _databaseService.GetDocumentDataByIdAsync(id);
                if (documentData == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูลเอกสารที่มี ID: {id}" });
                }

                // สร้าง PDF
                string fileName = await _pdfService.GeneratePdfFromDocumentData(id);
                if (string.IsNullOrEmpty(fileName))
                {
                    return StatusCode(500, new { message = "ไม่สามารถสร้างไฟล์ PDF ได้" });
                }

                // ส่งไฟล์กลับ
                string filePath = _pdfService.GetPdfFilePath(fileName);
                var fileStream = System.IO.File.OpenRead(filePath);
                return File(fileStream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for document data: {Id}", id);
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการสร้างไฟล์ PDF: {ex.Message}" });
            }
        }

        [HttpGet("financial-data/{id}")]
        public async Task<IActionResult> GetPdfFromFinancialData(string id)
        {
            try
            {
                // ตรวจสอบว่ามีผลลัพธ์ OCR หรือไม่
                var ocrResult = await _databaseService.GetOcrResultByIdAsync(id);
                if (ocrResult == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูล OCR ที่มี ID: {id}" });
                }

                if (ocrResult.FinancialData == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูลทางการเงินใน OCR ที่มี ID: {id}" });
                }

                // สร้าง PDF
                string fileName = await _pdfService.GeneratePdfFromFinancialData(id);
                if (string.IsNullOrEmpty(fileName))
                {
                    return StatusCode(500, new { message = "ไม่สามารถสร้างไฟล์ PDF ได้" });
                }

                // ส่งไฟล์กลับ
                string filePath = _pdfService.GetPdfFilePath(fileName);
                var fileStream = System.IO.File.OpenRead(filePath);
                return File(fileStream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for financial data: {Id}", id);
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการสร้างไฟล์ PDF: {ex.Message}" });
            }
        }
    }
}
