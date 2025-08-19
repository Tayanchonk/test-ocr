namespace OcrApi.Models
{
    public class FinancialDocumentData
    {
        public string? DocumentNumber { get; set; }
        public string? PersonName { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public List<ExpenseItem> ExpenseItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; } = "THB";
    }

    public class ExpenseItem
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; } = 1;
    }
}