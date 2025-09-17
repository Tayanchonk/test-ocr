using Microsoft.AspNetCore.Mvc;
using OcrApi.Models;
using OcrApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OcrApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(IDatabaseService databaseService, ILogger<DocumentController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentData>>> GetAllDocuments()
        {
            try
            {
                var documents = await _databaseService.GetAllDocumentDataAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document data");
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูลเอกสาร: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentData>> GetDocumentById(int id)
        {
            try
            {
                var document = await _databaseService.GetDocumentDataByIdAsync(id);
                if (document == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูลเอกสารที่มี ID: {id}" });
                }
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document data with ID: {Id}", id);
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูลเอกสาร: {ex.Message}" });
            }
        }

        [HttpGet("by-ocr-result/{ocrResultId}")]
        public async Task<ActionResult<IEnumerable<DocumentData>>> GetDocumentsByOcrResultId(string ocrResultId)
        {
            try
            {
                var documents = await _databaseService.GetAllDocumentDataAsync();
                var filteredDocuments = documents.Where(d => d.OcrResultId == ocrResultId).ToList();
                
                if (filteredDocuments.Count == 0)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูลเอกสารที่มี OCR Result ID: {ocrResultId}" });
                }
                
                return Ok(filteredDocuments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document data for OCR Result ID: {Id}", ocrResultId);
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูลเอกสาร: {ex.Message}" });
            }
        }

        [HttpGet("customs-receipts")]
        public async Task<ActionResult<IEnumerable<DocumentData>>> GetCustomsReceipts()
        {
            try
            {
                var documents = await _databaseService.GetAllDocumentDataAsync();
                var customsReceipts = documents.Where(d => d.IsCustomsReceipt).ToList();
                
                return Ok(customsReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customs receipt documents");
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูลใบเสร็จกรมศุลกากร: {ex.Message}" });
            }
        }
    }
}
