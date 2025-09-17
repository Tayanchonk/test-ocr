using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using OcrApi.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OcrApi.Services
{
    public interface IPdfService
    {
        Task<string> GeneratePdfFromOcrResult(string ocrResultId);
        Task<string> GeneratePdfFromDocumentData(int documentDataId);
        Task<string> GeneratePdfFromFinancialData(string ocrResultId);
        string GetPdfFilePath(string fileName);
    }

    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly string _pdfDirectory;
        private readonly string _fontDirectory;
        private PdfFont? _thaiFont;
        private readonly bool _useSarabun = true; // เปลี่ยนเป็น true เพื่อใช้ Sarabun แทน Noto Sans Thai

        public PdfService(ILogger<PdfService> logger, IDatabaseService databaseService, IWebHostEnvironment env)
        {
            _logger = logger;
            _databaseService = databaseService;
            
            // สร้างโฟลเดอร์สำหรับเก็บไฟล์ PDF
            _pdfDirectory = System.IO.Path.Combine(env.ContentRootPath, "wwwroot", "pdf");
            if (!Directory.Exists(_pdfDirectory))
            {
                Directory.CreateDirectory(_pdfDirectory);
            }

            // กำหนด path ของโฟลเดอร์ฟอนต์
            _fontDirectory = System.IO.Path.Combine(env.ContentRootPath, "Fonts");
        }

        // โหลดฟอนต์ภาษาไทย
        private PdfFont GetThaiFont()
        {
            try
            {
                if (_thaiFont == null)
                {
                    // ลำดับการเลือกฟอนต์
                    string[] fontOptions = _useSarabun 
                        ? new[] { "THSarabunNew.ttf", "THSarabun.ttf", "NotoSansThai.ttf", "Sarabun-Regular.ttf" }
                        : new[] { "NotoSansThai.ttf", "THSarabunNew.ttf", "THSarabun.ttf", "Sarabun-Regular.ttf" };
                    
                    PdfFont? font = null;
                    string fontUsed = string.Empty;
                    
                    // ลองใช้ฟอนต์ทั้งหมดตามลำดับที่กำหนด
                    foreach (string fontFile in fontOptions)
                    {
                        string fontPath = System.IO.Path.Combine(_fontDirectory, fontFile);
                        if (File.Exists(fontPath))
                        {
                            try
                            {
                                _logger.LogInformation("Trying to load Thai font: {FontPath}", fontPath);
                                font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                                fontUsed = fontFile;
                                _logger.LogInformation("Successfully loaded Thai font: {FontFile}", fontFile);
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to load font {FontFile}: {Message}", fontFile, ex.Message);
                            }
                        }
                    }
                    
                    if (font != null)
                    {
                        _thaiFont = font;
                        _logger.LogInformation("Using Thai font: {FontUsed}", fontUsed);
                    }
                    else
                    {
                        _logger.LogWarning("No Thai fonts could be loaded. Using default font.");
                        _thaiFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.UTF8);
                    }
                }
                
                return _thaiFont;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetThaiFont method: {Message}", ex.Message);
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.UTF8);
            }
        }

        public string GetPdfFilePath(string fileName)
        {
            return System.IO.Path.Combine(_pdfDirectory, fileName);
        }

        // เมธอดสำหรับจัดการข้อความภาษาไทยให้แสดงผลถูกต้อง
        private string FixThaiTextSpacing(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // แปลงข้อความที่มีช่องว่างระหว่างตัวอักษรไทยให้ติดกัน
            // อย่างไรก็ตาม รักษาช่องว่างระหว่างคำ และรักษาตัวเลขและอักขระพิเศษ
            StringBuilder sb = new StringBuilder();
            bool lastWasThai = false;
            
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isThai = IsThaiChar(c);
                bool isDigit = char.IsDigit(c);
                bool isSpecial = IsSpecialChar(c);
                
                if (c == ' ')
                {
                    // ถ้าอักขระก่อนหน้าและถัดไปเป็นไทย ให้ข้ามช่องว่างไป
                    bool nextIsThai = (i < text.Length - 1) && IsThaiChar(text[i + 1]);
                    
                    if (lastWasThai && nextIsThai)
                    {
                        // ข้ามช่องว่างระหว่างตัวอักษรไทย
                        continue;
                    }
                    else
                    {
                        // เก็บช่องว่างระหว่างคำหรือระหว่างภาษาไทยกับอักขระอื่น
                        sb.Append(c);
                    }
                }
                else
                {
                    // เพิ่มตัวอักษรทุกตัวลงไป
                    sb.Append(c);
                    lastWasThai = isThai;
                }
            }
            
            return sb.ToString();
        }
        
        // ตรวจสอบว่าเป็นตัวอักษรไทยหรือไม่
        private bool IsThaiChar(char c)
        {
            // ตัวอักษรไทยอยู่ในช่วง Unicode U+0E00-U+0E7F
            return (c >= 0x0E00 && c <= 0x0E7F);
        }
        
        // ตรวจสอบอักขระพิเศษ
        private bool IsSpecialChar(char c)
        {
            // อักขระพิเศษที่ต้องการรักษาไว้
            string specialChars = ".,()[]{}:;-_+=/*&^%$#@!?|\\<>\"'";
            return specialChars.Contains(c);
        }

        public async Task<string> GeneratePdfFromOcrResult(string ocrResultId)
        {
            try
            {
                var ocrResult = await _databaseService.GetOcrResultByIdAsync(ocrResultId);
                if (ocrResult == null)
                {
                    _logger.LogWarning("OCR result not found: {Id}", ocrResultId);
                    return null;
                }

                // สร้างชื่อไฟล์ PDF
                string fileName = $"ocr-result-{ocrResultId}.pdf";
                string filePath = GetPdfFilePath(fileName);

                // สร้างไฟล์ PDF
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        using (Document document = new Document(pdf))
                        {
                            // โหลดฟอนต์ภาษาไทย
                            PdfFont thaiFont = GetThaiFont();
                            
                            // เพิ่มหัวข้อ
                            document.Add(new Paragraph("OCR Result")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20)
                                .SetFont(thaiFont));

                            // เพิ่มข้อมูลทั่วไป
                            document.Add(new Paragraph($"ID: {ocrResult.Id}").SetFont(thaiFont));
                            document.Add(new Paragraph($"File Name: {ocrResult.FileName}").SetFont(thaiFont));
                            document.Add(new Paragraph($"File Type: {ocrResult.FileType}").SetFont(thaiFont));
                            document.Add(new Paragraph($"Status: {ocrResult.Status}").SetFont(thaiFont));
                            document.Add(new Paragraph($"Processed At: {ocrResult.ProcessedAt}").SetFont(thaiFont));
                            document.Add(new Paragraph($"Confidence Score: {ocrResult.ConfidenceScore:P2}").SetFont(thaiFont));

                            // เพิ่มเส้นคั่น
                            document.Add(new LineSeparator(new SolidLine()));

                            // เพิ่มข้อความที่สกัดได้
                            Paragraph extractedTextHeader = new Paragraph("Extracted Text:");
                            extractedTextHeader.SetFontSize(16);
                            extractedTextHeader.SetBold();
                            extractedTextHeader.SetFont(thaiFont);
                            document.Add(extractedTextHeader);
                            
                            // แก้ไขปัญหาช่องว่างในข้อความภาษาไทย
                            string fixedText = FixThaiTextSpacing(ocrResult.ExtractedText ?? "No text extracted");
                            document.Add(new Paragraph(fixedText).SetFont(thaiFont));

                            // เพิ่มข้อมูลทางการเงิน (ถ้ามี)
                            if (ocrResult.FinancialData != null)
                            {
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph financialDataHeader = new Paragraph("Financial Data:");
                                financialDataHeader.SetFontSize(16);
                                financialDataHeader.SetBold();
                                document.Add(financialDataHeader);

                                document.Add(new Paragraph($"Document Type: {ocrResult.FinancialData.DocumentType ?? "N/A"}"));
                                document.Add(new Paragraph($"Document Number: {ocrResult.FinancialData.DocumentNumber ?? "N/A"}"));
                                document.Add(new Paragraph($"Document Date: {(ocrResult.FinancialData.DocumentDate.HasValue ? ocrResult.FinancialData.DocumentDate.Value.ToString("dd/MM/yyyy") : "N/A")}"));
                                document.Add(new Paragraph($"Person Name: {ocrResult.FinancialData.PersonName ?? "N/A"}"));
                                document.Add(new Paragraph($"Tax ID: {ocrResult.FinancialData.TaxId ?? "N/A"}"));
                                document.Add(new Paragraph($"Total Amount: {ocrResult.FinancialData.TotalAmount:N2} {ocrResult.FinancialData.Currency ?? "THB"}"));
                                
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.TotalAmountText))
                                {
                                    // แก้ไขช่องว่างในตัวหนังสือจำนวนเงินภาษาไทย
                                    string fixedAmountText = FixThaiTextSpacing(ocrResult.FinancialData.TotalAmountText);
                                    document.Add(new Paragraph($"Total Amount (Text): {fixedAmountText}").SetFont(thaiFont));
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation("PDF generated successfully: {FilePath}", filePath);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from OCR result: {Id}", ocrResultId);
                return null;
            }
        }

        public async Task<string> GeneratePdfFromDocumentData(int documentDataId)
        {
            try
            {
                var documentData = await _databaseService.GetDocumentDataByIdAsync(documentDataId);
                if (documentData == null)
                {
                    _logger.LogWarning("Document data not found: {Id}", documentDataId);
                    return null;
                }

                // สร้างชื่อไฟล์ PDF
                string fileName = $"document-data-{documentDataId}.pdf";
                string filePath = GetPdfFilePath(fileName);

                // สร้างไฟล์ PDF
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        using (Document document = new Document(pdf))
                        {
                            // โหลดฟอนต์ภาษาไทย
                            PdfFont thaiFont = GetThaiFont();
                            
                            // เพิ่มหัวข้อ
                            string title = FixThaiTextSpacing(documentData.IsCustomsReceipt ? "ใบเสร็จกรมศุลกากร" : (documentData.DocumentType ?? "เอกสาร"));
                            document.Add(new Paragraph(title)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20)
                                .SetFont(thaiFont));

                            // เพิ่มข้อมูลทั่วไป
                            if (!string.IsNullOrEmpty(documentData.DocumentNumber))
                            {
                                document.Add(new Paragraph($"เลขที่เอกสาร: {documentData.DocumentNumber}").SetFont(thaiFont));
                            }
                            
                            if (documentData.DocumentDate.HasValue)
                            {
                                document.Add(new Paragraph($"วันที่เอกสาร: {documentData.DocumentDate.Value.ToString("dd/MM/yyyy")}").SetFont(thaiFont));
                            }
                            
                            if (!string.IsNullOrEmpty(documentData.Department))
                            {
                                string dept = FixThaiTextSpacing(documentData.Department);
                                document.Add(new Paragraph($"หน่วยงาน: {dept}").SetFont(thaiFont));
                            }

                            // เพิ่มข้อมูลบุคคล/บริษัท
                            if (!string.IsNullOrEmpty(documentData.PersonName) || !string.IsNullOrEmpty(documentData.TaxId))
                            {
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph personHeader = new Paragraph(FixThaiTextSpacing("ข้อมูลบุคคล/บริษัท:"));
                                personHeader.SetFontSize(16);
                                personHeader.SetBold();
                                personHeader.SetFont(thaiFont);
                                document.Add(personHeader);
                                
                                if (!string.IsNullOrEmpty(documentData.PersonName))
                                {
                                    string name = FixThaiTextSpacing(documentData.PersonName);
                                    document.Add(new Paragraph(FixThaiTextSpacing($"ชื่อ: {name}")).SetFont(thaiFont));
                                }
                                
                                if (!string.IsNullOrEmpty(documentData.TaxId))
                                {
                                    document.Add(new Paragraph(FixThaiTextSpacing($"เลขประจำตัวผู้เสียภาษี: {documentData.TaxId}")).SetFont(thaiFont));
                                }
                                
                                if (!string.IsNullOrEmpty(documentData.Address))
                                {
                                    string address = FixThaiTextSpacing(documentData.Address);
                                    document.Add(new Paragraph(FixThaiTextSpacing($"ที่อยู่: {address}")).SetFont(thaiFont));
                                }
                            }

                            // เพิ่มข้อมูลใบเสร็จศุลกากร (ถ้ามี)
                            if (documentData.IsCustomsReceipt)
                            {
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph customsHeader = new Paragraph("ข้อมูลศุลกากร:");
                                customsHeader.SetFontSize(16);
                                customsHeader.SetBold();
                                customsHeader.SetFont(thaiFont);
                                document.Add(customsHeader);
                                
                                if (!string.IsNullOrEmpty(documentData.CustomsDeclarationNumber))
                                {
                                    document.Add(new Paragraph($"เลขที่ใบขนสินค้า: {documentData.CustomsDeclarationNumber}").SetFont(thaiFont));
                                }
                                
                                if (!string.IsNullOrEmpty(documentData.CustomsPaymentNumber))
                                {
                                    document.Add(new Paragraph($"เลขที่ชำระอากร: {documentData.CustomsPaymentNumber}").SetFont(thaiFont));
                                }
                                
                                if (!string.IsNullOrEmpty(documentData.CustomsPaymentDate))
                                {
                                    document.Add(new Paragraph($"วันที่ชำระอากร: {documentData.CustomsPaymentDate}").SetFont(thaiFont));
                                }
                            }

                            // เพิ่มข้อมูลทางการเงิน
                            document.Add(new LineSeparator(new SolidLine()));
                            
                            Paragraph financialHeader = new Paragraph("ข้อมูลทางการเงิน:");
                            financialHeader.SetFontSize(16);
                            financialHeader.SetBold();
                            financialHeader.SetFont(thaiFont);
                            document.Add(financialHeader);
                            
                            // แสดงรายละเอียดค่าใช้จ่าย
                            if (documentData.ImportDuty.HasValue && documentData.ImportDuty.Value > 0)
                            {
                                document.Add(new Paragraph($"อากรขาเข้า: {documentData.ImportDuty.Value:N2} บาท").SetFont(thaiFont));
                            }
                            
                            if (documentData.Vat.HasValue && documentData.Vat.Value > 0)
                            {
                                document.Add(new Paragraph($"ภาษีมูลค่าเพิ่ม: {documentData.Vat.Value:N2} บาท").SetFont(thaiFont));
                            }
                            
                            if (documentData.OtherFees.HasValue && documentData.OtherFees.Value > 0)
                            {
                                document.Add(new Paragraph($"ค่าธรรมเนียมอื่นๆ: {documentData.OtherFees.Value:N2} บาท").SetFont(thaiFont));
                            }
                            
                            // แสดงยอดรวม
                            Paragraph totalAmount = new Paragraph($"ยอดรวมทั้งสิ้น: {documentData.TotalAmount:N2} {documentData.Currency ?? "บาท"}");
                            totalAmount.SetFontSize(14);
                            totalAmount.SetBold();
                            totalAmount.SetFont(thaiFont);
                            document.Add(totalAmount);
                            
                            if (!string.IsNullOrEmpty(documentData.TotalAmountText))
                            {
                                document.Add(new Paragraph($"({documentData.TotalAmountText})").SetFont(thaiFont));
                            }

                            // เพิ่มข้อมูลระบบ
                            document.Add(new LineSeparator(new SolidLine()));
                            document.Add(new Paragraph($"วันที่สร้าง: {documentData.CreatedAt}").SetFont(thaiFont));
                            if (documentData.UpdatedAt.HasValue)
                            {
                                document.Add(new Paragraph($"วันที่แก้ไขล่าสุด: {documentData.UpdatedAt.Value}"));
                            }
                        }
                    }
                }

                _logger.LogInformation("PDF generated successfully: {FilePath}", filePath);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from document data: {Id}", documentDataId);
                return null;
            }
        }

        public async Task<string> GeneratePdfFromFinancialData(string ocrResultId)
        {
            try
            {
                var ocrResult = await _databaseService.GetOcrResultByIdAsync(ocrResultId);
                if (ocrResult == null)
                {
                    _logger.LogWarning("OCR result not found: {Id}", ocrResultId);
                    return null;
                }

                // สร้างชื่อไฟล์ PDF
                string fileName = $"financial-data-{ocrResultId}.pdf";
                string filePath = GetPdfFilePath(fileName);

                // สร้างไฟล์ PDF
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        using (Document document = new Document(pdf))
                        {
                            // เพิ่มหัวข้อ
                            document.Add(new Paragraph("ข้อมูลทางการเงิน")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20));

                            // เพิ่มข้อมูลทั่วไป
                            if (ocrResult.FinancialData != null)
                            {
                                // ข้อมูลองค์กร
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.Department))
                                {
                                    document.Add(new Paragraph($"หน่วยงาน: {ocrResult.FinancialData.Department}"));
                                }

                                // ประเภทเอกสาร
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.DocumentType))
                                {
                                    document.Add(new Paragraph($"ประเภทเอกสาร: {ocrResult.FinancialData.DocumentType}"));
                                }

                                // เพิ่มข้อมูลอ้างอิง
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph referenceHeader = new Paragraph("ข้อมูลอ้างอิง:");
                                referenceHeader.SetFontSize(16);
                                referenceHeader.SetBold();
                                document.Add(referenceHeader);

                                // เลขที่เอกสาร
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.DocumentNumber))
                                {
                                    document.Add(new Paragraph($"เลขที่เอกสาร: {ocrResult.FinancialData.DocumentNumber}"));
                                }

                                // เลขประจำตัวผู้เสียภาษี
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.TaxId))
                                {
                                    document.Add(new Paragraph($"เลขประจำตัวผู้เสียภาษี: {ocrResult.FinancialData.TaxId}"));
                                }

                                // ชื่อผู้นำของเข้า/ผู้ส่งของออก
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.PersonName))
                                {
                                    document.Add(new Paragraph($"ชื่อ: {ocrResult.FinancialData.PersonName}"));
                                }

                                // เลขที่ใบขนสินค้า
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.CustomsDeclarationNumber))
                                {
                                    document.Add(new Paragraph($"เลขที่ใบขนสินค้า: {ocrResult.FinancialData.CustomsDeclarationNumber}"));
                                }

                                // เลขที่ชำระอากร/วันเดือนปี
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.CustomsPaymentNumber))
                                {
                                    document.Add(new Paragraph($"เลขที่ชำระอากร: {ocrResult.FinancialData.CustomsPaymentNumber}"));
                                }

                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.CustomsPaymentDate))
                                {
                                    document.Add(new Paragraph($"วันที่ชำระอากร: {ocrResult.FinancialData.CustomsPaymentDate}"));
                                }

                                // เพิ่มข้อมูลรายการ
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph itemsHeader = new Paragraph("รายการค่าใช้จ่าย:");
                                itemsHeader.SetFontSize(16);
                                itemsHeader.SetBold();
                                document.Add(itemsHeader);

                                // อากรขาเข้า
                                if (ocrResult.FinancialData.ImportDuty.HasValue && ocrResult.FinancialData.ImportDuty.Value > 0)
                                {
                                    document.Add(new Paragraph($"อากรขาเข้า: {ocrResult.FinancialData.ImportDuty.Value:N2} บาท"));
                                }

                                // ภาษีมูลค่าเพิ่ม
                                if (ocrResult.FinancialData.Vat.HasValue && ocrResult.FinancialData.Vat.Value > 0)
                                {
                                    document.Add(new Paragraph($"ภาษีมูลค่าเพิ่ม: {ocrResult.FinancialData.Vat.Value:N2} บาท"));
                                }

                                // ค่าธรรมเนียมอื่นๆ
                                if (ocrResult.FinancialData.OtherFees?.Count > 0)
                                {
                                    document.Add(new Paragraph("ค่าธรรมเนียมอื่นๆ:"));
                                    foreach (var fee in ocrResult.FinancialData.OtherFees)
                                    {
                                        document.Add(new Paragraph($"    - {fee.Key}: {fee.Value:N2} บาท"));
                                    }
                                }

                                // เพิ่มยอดรวม
                                document.Add(new LineSeparator(new SolidLine()));
                                
                                Paragraph totalHeader = new Paragraph("ยอดรวม:");
                                totalHeader.SetFontSize(16);
                                totalHeader.SetBold();
                                document.Add(totalHeader);

                                // จำนวนเงินรวม
                                Paragraph totalAmount = new Paragraph($"ยอดรวมทั้งสิ้น: {ocrResult.FinancialData.TotalAmount:N2} {ocrResult.FinancialData.Currency ?? "บาท"}");
                                totalAmount.SetFontSize(14);
                                totalAmount.SetBold();
                                document.Add(totalAmount);

                                // จำนวนเงินเป็นตัวอักษร
                                if (!string.IsNullOrEmpty(ocrResult.FinancialData.TotalAmountText))
                                {
                                    document.Add(new Paragraph($"({ocrResult.FinancialData.TotalAmountText})"));
                                }
                            }
                            else
                            {
                                document.Add(new Paragraph("ไม่พบข้อมูลทางการเงิน"));
                            }
                        }
                    }
                }

                _logger.LogInformation("Financial data PDF generated successfully: {FilePath}", filePath);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from financial data: {Id}", ocrResultId);
                return null;
            }
        }
    }
}
