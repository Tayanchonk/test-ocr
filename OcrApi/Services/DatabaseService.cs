using Microsoft.EntityFrameworkCore;
using OcrApi.Data;
using OcrApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OcrApi.Services
{
    public interface IDatabaseService
    {
        Task<bool> TestConnectionAsync();
        string GetLastError();
        Task<OcrResult> SaveOcrResultAsync(OcrResult result);
        Task<OcrResult> GetOcrResultByIdAsync(string id);
        Task<List<OcrResult>> GetAllOcrResultsAsync();
        Task<CustomsReceipt> SaveCustomsReceiptAsync(CustomsReceipt receipt);
        Task<DocumentData> SaveDocumentDataAsync(DocumentData documentData);
        Task<DocumentData> CreateDocumentDataFromOcrResultAsync(OcrResult result, CustomsReceipt customsReceipt = null);
        Task<List<DocumentData>> GetAllDocumentDataAsync();
        Task<DocumentData> GetDocumentDataByIdAsync(int id);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;
        private string _lastError;

        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // ทดสอบการเชื่อมต่อกับฐานข้อมูล
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Database connection error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public string GetLastError()
        {
            return _lastError ?? "ไม่มีข้อผิดพลาด";
        }

        public async Task<OcrResult> SaveOcrResultAsync(OcrResult result)
        {
            try
            {
                // ตรวจสอบว่ามีข้อมูลในฐานข้อมูลแล้วหรือไม่
                var existingResult = await _context.OcrResults.FindAsync(result.Id);
                
                if (existingResult != null)
                {
                    // อัปเดตข้อมูลที่มีอยู่แล้ว
                    _context.Entry(existingResult).CurrentValues.SetValues(result);
                }
                else
                {
                    // เพิ่มข้อมูลใหม่
                    await _context.OcrResults.AddAsync(result);
                }
                
                await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Error saving OCR result: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<OcrResult> GetOcrResultByIdAsync(string id)
        {
            return await _context.OcrResults.FindAsync(id);
        }

        public async Task<List<OcrResult>> GetAllOcrResultsAsync()
        {
            return await _context.OcrResults.OrderByDescending(r => r.ProcessedAt).ToListAsync();
        }

        public async Task<CustomsReceipt> SaveCustomsReceiptAsync(CustomsReceipt receipt)
        {
            try
            {
                // ตรวจสอบว่ามีข้อมูลในฐานข้อมูลแล้วหรือไม่
                var existingReceipt = await _context.CustomsReceipts
                    .Include(c => c.Reference)
                    .Include(c => c.Items)
                    .Include(c => c.Total)
                    .Include(c => c.Sign)
                    .FirstOrDefaultAsync(c => c.Id == receipt.Id);
                
                if (existingReceipt != null)
                {
                    // อัปเดตข้อมูลที่มีอยู่แล้ว
                    _context.Entry(existingReceipt).CurrentValues.SetValues(receipt);
                    _context.Entry(existingReceipt.Reference).CurrentValues.SetValues(receipt.Reference);
                    _context.Entry(existingReceipt.Items).CurrentValues.SetValues(receipt.Items);
                    _context.Entry(existingReceipt.Total).CurrentValues.SetValues(receipt.Total);
                    _context.Entry(existingReceipt.Sign).CurrentValues.SetValues(receipt.Sign);
                }
                else
                {
                    // เพิ่มข้อมูลใหม่
                    await _context.CustomsReceipts.AddAsync(receipt);
                }
                
                await _context.SaveChangesAsync();
                return receipt;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Error saving Customs Receipt: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<DocumentData> SaveDocumentDataAsync(DocumentData documentData)
        {
            try
            {
                // ตรวจสอบว่ามีข้อมูลในฐานข้อมูลแล้วหรือไม่
                var existingDocument = await _context.DocumentData.FindAsync(documentData.Id);
                
                if (existingDocument != null)
                {
                    // อัปเดตข้อมูลที่มีอยู่แล้ว
                    documentData.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(existingDocument).CurrentValues.SetValues(documentData);
                }
                else
                {
                    // เพิ่มข้อมูลใหม่
                    await _context.DocumentData.AddAsync(documentData);
                }
                
                await _context.SaveChangesAsync();
                return documentData;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Error saving Document Data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<DocumentData> CreateDocumentDataFromOcrResultAsync(OcrResult result, CustomsReceipt customsReceipt = null)
        {
            try
            {
                // สร้าง DocumentData จาก OcrResult
                var documentData = DocumentData.FromOcrResult(result, customsReceipt);
                
                // บันทึกลงฐานข้อมูล
                await _context.DocumentData.AddAsync(documentData);
                await _context.SaveChangesAsync();
                
                return documentData;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Error creating Document Data from OCR result: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<List<DocumentData>> GetAllDocumentDataAsync()
        {
            return await _context.DocumentData.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }

        public async Task<DocumentData> GetDocumentDataByIdAsync(int id)
        {
            return await _context.DocumentData.FindAsync(id);
        }
    }
}
