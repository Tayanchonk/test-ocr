using Microsoft.AspNetCore.Mvc;
using OcrApi.Services;
using Microsoft.Extensions.Configuration;
using OcrApi.Models;

namespace OcrApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IConfiguration _configuration;

        public DatabaseController(IDatabaseService databaseService, IConfiguration configuration)
        {
            _databaseService = databaseService;
            _configuration = configuration;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            bool canConnect = await _databaseService.TestConnectionAsync();
            
            if (canConnect)
            {
                return Ok(new { message = "เชื่อมต่อกับฐานข้อมูล SQL Server สำเร็จ!" });
            }
            else
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "ไม่พบ connection string";
                return StatusCode(500, new { 
                    message = "ไม่สามารถเชื่อมต่อกับฐานข้อมูล SQL Server ได้", 
                    error = _databaseService.GetLastError(),
                    connectionString = connectionString
                });
            }
        }

        [HttpGet("ocr-results")]
        public async Task<ActionResult<IEnumerable<OcrResult>>> GetAllOcrResults()
        {
            try
            {
                var results = await _databaseService.GetAllOcrResultsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูล: {ex.Message}" });
            }
        }

        [HttpGet("ocr-results/{id}")]
        public async Task<ActionResult<OcrResult>> GetOcrResultById(string id)
        {
            try
            {
                var result = await _databaseService.GetOcrResultByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = $"ไม่พบข้อมูล OCR ที่มี ID: {id}" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงข้อมูล: {ex.Message}" });
            }
        }
    }
}
