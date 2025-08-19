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

        public OcrController(IProcessingService processingService, ILogger<OcrController> logger)
        {
            _processingService = processingService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a file for general OCR text extraction
        /// </summary>
        /// <param name="file">PDF or image file to process</param>
        /// <param name="language">OCR language (default: tha+eng for Thai and English)</param>
        /// <returns>Processing ID</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<FileUploadResponse>> UploadFile(IFormFile file, [FromForm] string language = "tha+eng")
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
                
                return Ok(new FileUploadResponse
                {
                    Id = processingId,
                    Message = "File uploaded and processing started",
                    Status = ProcessingStatus.Processing
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                return StatusCode(500, new { message = "Internal server error during file processing" });
            }
        }

        /// <summary>
        /// Upload a file for financial document data extraction
        /// </summary>
        /// <param name="file">PDF or image file containing financial document</param>
        /// <param name="language">OCR language (default: tha+eng for Thai and English)</param>
        /// <returns>Processing ID</returns>
        [HttpPost("upload-financial")]
        public async Task<ActionResult<FileUploadResponse>> UploadFinancialDocument(IFormFile file, [FromForm] string language = "tha+eng")
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
                var processingId = await _processingService.ProcessFileAsync(file, true, language);
                
                return Ok(new FileUploadResponse
                {
                    Id = processingId,
                    Message = "Financial document uploaded and processing started",
                    Status = ProcessingStatus.Processing
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading financial document {FileName}", file.FileName);
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

            return Ok(result);
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
            return Ok(results);
        }
    }
}