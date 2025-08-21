using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OcrApi.Models
{
    public class DocumentData
    {
        [Key]
        public int Id { get; set; }
        
        public string OcrResultId { get; set; }
        
        // ข้อมูลทั่วไปของเอกสาร
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        
        // ข้อมูลบุคคล/บริษัท
        public string? PersonName { get; set; }
        public string? TaxId { get; set; }
        public string? Address { get; set; }
        
        // ข้อมูลทางการเงิน
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }
        
        // ข้อมูลสำหรับใบเสร็จศุลกากร
        public string? CustomsDeclarationNumber { get; set; }
        public string? CustomsPaymentNumber { get; set; }
        public string? CustomsPaymentDate { get; set; }
        public decimal? ImportDuty { get; set; }
        public decimal? Vat { get; set; }
        public decimal? OtherFees { get; set; }
        
        // ข้อมูลเพิ่มเติม
        public string? Department { get; set; }
        public string? TotalAmountText { get; set; }
        public bool IsCustomsReceipt { get; set; }
        
        // ข้อมูลระบบ
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // สร้างจาก OcrResult และ CustomsReceipt
        public static DocumentData FromOcrResult(OcrResult result, CustomsReceipt customsReceipt = null)
        {
            var doc = new DocumentData
            {
                OcrResultId = result.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            // ถ้ามี FinancialData ให้ใช้ข้อมูลจาก FinancialData
            if (result.FinancialData != null)
            {
                doc.DocumentType = result.FinancialData.DocumentType;
                doc.DocumentNumber = result.FinancialData.DocumentNumber;
                doc.DocumentDate = result.FinancialData.DocumentDate;
                doc.ReferenceNumber = result.FinancialData.ReferenceNumber;
                doc.PersonName = result.FinancialData.PersonName;
                doc.TaxId = result.FinancialData.TaxId;
                doc.TotalAmount = result.FinancialData.TotalAmount;
                doc.Currency = result.FinancialData.Currency;
                doc.Department = result.FinancialData.Department;
                doc.TotalAmountText = result.FinancialData.TotalAmountText;
                doc.CustomsDeclarationNumber = result.FinancialData.CustomsDeclarationNumber;
                doc.CustomsPaymentNumber = result.FinancialData.CustomsPaymentNumber;
                doc.CustomsPaymentDate = result.FinancialData.CustomsPaymentDate;
                doc.ImportDuty = result.FinancialData.ImportDuty;
                doc.Vat = result.FinancialData.Vat;
                
                // รวมค่าธรรมเนียมอื่นๆ ถ้ามี
                if (result.FinancialData.OtherFees?.Count > 0)
                {
                    doc.OtherFees = result.FinancialData.OtherFees.Values.Sum();
                }
            }
            
            // ถ้ามี CustomsReceipt ให้ระบุเป็นใบเสร็จศุลกากร
            if (customsReceipt != null)
            {
                doc.IsCustomsReceipt = true;
                doc.DocumentType = customsReceipt.DocumentType;
                doc.Department = customsReceipt.Organization;
                doc.DocumentNumber = customsReceipt.Reference.Skc;
                doc.TaxId = customsReceipt.Reference.TaxId;
                doc.PersonName = customsReceipt.Reference.DeclarantName;
                doc.CustomsDeclarationNumber = customsReceipt.Reference.DeclarationNo;
                doc.CustomsPaymentNumber = customsReceipt.Reference.PaymentRef;
                doc.CustomsPaymentDate = customsReceipt.Reference.PaymentDate;
                doc.ImportDuty = customsReceipt.Items.ImportDuty;
                doc.Vat = customsReceipt.Items.Vat;
                doc.OtherFees = customsReceipt.Items.Other;
                doc.TotalAmount = customsReceipt.Total.Amount;
                doc.TotalAmountText = customsReceipt.Total.AmountText;
            }
            
            return doc;
        }
    }
}
