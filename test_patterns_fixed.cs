using System;
using System.Text.RegularExpressions;

namespace TestFinancialDataFixed
{
    class Program
    {
        static void Main(string[] args)
        {
            // ข้อมูลดิบจาก OCR ที่ได้มา
            string extractedText = @"ล ํ า อ า ก ร ษา ข้า 86061.00
ค ่ า ภา ษี ย ุ ล ค ่ า เพ ิ ่ ม 6626700
=300
fuom| 15532600 §";

            Console.WriteLine("กำลังทดสอบ patterns ใหม่...");
            Console.WriteLine("ข้อมูลทดสอบ:");
            Console.WriteLine(extractedText);
            Console.WriteLine("\n=================================================================");
            
            // ทดสอบการดึง Import Duty
            Console.WriteLine("\n=== ทดสอบการดึง Import Duty ===");
            var importDutyMatch = Regex.Match(extractedText, @"ล\s*ํ\s*า\s*อ\s*า\s*ก\s*ร\s*ษา\s*ข้า\s*(\d+\.?\d*)");
            if (importDutyMatch.Success)
            {
                decimal importDuty = decimal.Parse(importDutyMatch.Groups[1].Value);
                Console.WriteLine($"✅ พบ Import Duty: {importDuty:F2}");
            }
            else
            {
                Console.WriteLine("❌ ไม่พบ Import Duty");
            }
            
            // ทดสอบการดึง VAT
            Console.WriteLine("\n=== ทดสอบการดึง VAT ===");
            var vatMatch = Regex.Match(extractedText, @"ค\s*่\s*า\s*ภา\s*ษี\s*ย\s*ุ\s*ล\s*ค\s*่\s*า\s*เพ\s*ิ\s*่\s*ม\s*(\d+\.?\d*)");
            if (vatMatch.Success)
            {
                decimal vat = decimal.Parse(vatMatch.Groups[1].Value);
                Console.WriteLine($"✅ พบ VAT: {vat:F2}");
            }
            else
            {
                Console.WriteLine("❌ ไม่พบ VAT");
            }
            
            // ทดสอบการดึง Total Amount
            Console.WriteLine("\n=== ทดสอบการดึง Total Amount ===");
            var totalMatch = Regex.Match(extractedText, @"fuom\|\s*(\d+)\s*§");
            if (totalMatch.Success)
            {
                decimal total = decimal.Parse(totalMatch.Groups[1].Value);
                Console.WriteLine($"✅ พบ Total Amount: {total:F2}");
            }
            else
            {
                Console.WriteLine("❌ ไม่พบ Total Amount");
            }
            
            Console.WriteLine("\nการทดสอบเสร็จสิ้น");
        }
    }
}
