using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OcrApi.Models
{
    public class FinancialDocumentData
    {
        public int Id { get; set; } // เพิ่ม primary key
        public string? DocumentNumber { get; set; }
        public string? PersonName { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TaxId { get; set; }
        public DateTime? DocumentDate { get; set; }
        
        // ไม่นำ List ไปสร้างเป็นคอลัมน์ในฐานข้อมูล
        [NotMapped]
        public List<ExpenseItem> ExpenseItems { get; set; } = new();
        
        public decimal TotalAmount { get; set; }
        public string? TotalAmountText { get; set; }
        public string? Department { get; set; }
        public string? Currency { get; set; } = "THB";
        public string? DocumentType { get; set; }
        
        // เพิ่มเติมสำหรับใบเสร็จศุลกากร
        public string? CustomsDeclarationNumber { get; set; }
        public string? CustomsPaymentNumber { get; set; }
        public string? CustomsPaymentDate { get; set; }
        public decimal? ImportDuty { get; set; }
        public decimal? Vat { get; set; }
        
        // ไม่นำ Dictionary ไปสร้างเป็นคอลัมน์ในฐานข้อมูล
        [NotMapped]
        public Dictionary<string, decimal> OtherFees { get; set; } = new();
    }

    public class ExpenseItem
    {
        public int Id { get; set; } // เพิ่ม primary key
        public int FinancialDocumentDataId { get; set; } // เพิ่ม foreign key
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; } = 1;
    }
}