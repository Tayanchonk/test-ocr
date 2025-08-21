namespace OcrApi.Models
{
    public class CustomsReceipt
    {
        public int Id { get; set; }  // เพิ่ม primary key
        public string Organization { get; set; } = "กรมศุลกากร";
        public string DocumentType { get; set; } = "ใบเสร็จรับเงิน";
        
        public ReferenceInfo Reference { get; set; } = new ReferenceInfo();
        public ItemsInfo Items { get; set; } = new ItemsInfo();
        public TotalInfo Total { get; set; } = new TotalInfo();
        public SignInfo Sign { get; set; } = new SignInfo();
    }

    public class ReferenceInfo
    {
        public int Id { get; set; }  // เพิ่ม primary key
        public int CustomsReceiptId { get; set; }  // เพิ่ม foreign key
        public string? Skc { get; set; }
        public string? Hawb { get; set; }
        public string? VehicleNo { get; set; }
        public string? TaxId { get; set; }
        public string? ImportExportDate { get; set; }
        public string? DeclarantName { get; set; }
        public string? DeclarationNo { get; set; }
        public string? PaymentRef { get; set; }
        public string? PaymentDate { get; set; }
    }

    public class ItemsInfo
    {
        public int Id { get; set; }  // เพิ่ม primary key
        public int CustomsReceiptId { get; set; }  // เพิ่ม foreign key
        public decimal ImportDuty { get; set; }
        public decimal Vat { get; set; }
        public decimal Other { get; set; }
    }

    public class TotalInfo
    {
        public int Id { get; set; }  // เพิ่ม primary key
        public int CustomsReceiptId { get; set; }  // เพิ่ม foreign key
        public decimal Amount { get; set; }
        public string? AmountText { get; set; }
    }

    public class SignInfo
    {
        public int Id { get; set; }  // เพิ่ม primary key
        public int CustomsReceiptId { get; set; }  // เพิ่ม foreign key
        public string? Receiver { get; set; }
        public string? Officer { get; set; }
    }
}
