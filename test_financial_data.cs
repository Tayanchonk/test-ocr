using System;
using System.Text.RegularExpressions;

namespace TestFinancialData
{
    class Program
    {
        static void Main(string[] args)
        {
            // ข้อมูลดิบจาก OCR ที่ได้มา
            string extractedText = @"6103210097370000010.

 

เ "" เก ค า ร
. . ใบ เส ร ็ จ ร ั บ เง ิ น
ศก. 123
ก ร ม ศุ ล ก า ก ร .
เล ข ป ร ะ จ ํ า ต ั ว ผู ้ เส ี ย ภา ษี ย า ก ร 1409900960780/000000 , ล ง บ ั ญ ชี บ ํ า ม ว ั น
ชื ่ อ ผู ้ น ํ า ขอ ง เข ้ า / ผ ู ้ ส ่ ง ขอ ง อ อ ก WA เภ ศ ส ุ ด ฯ อ ม ร ร ั ต น พ ง ศ์

เส ข ท ี ่ ใบ ขน ส ิ น ค ้ า ศิ 021-0610307117 (1193)

เล ข ท ี ่ ชํา ร ะ อ า ก ร / ว ั น เด ื อ น ป ี 1190-133687/21-03-61

 

 

 

 

 

 

 

ได ้ ร ั บ ง ิ น ต า ม ร า ย ก า ร ข้ า ง ถ่าง น ี ้ ไว ้ แล ้ ว ท ี ่ ชํา ร ะ ต า บ ส ํ า แด ง (บ า ท ) ท ี ่ ว า ง ป ร
ล ํ า อ า ก ร ษา ข้า 86061.00
ค ่ า ภา ษี ย ุ ล ค ่ า เพ ิ ่ ม 6626700
=300
fuom| 15532600 §

€ ot T

sy จ ํ า แว บ ญี น ดา ขี ม ล ค พื อ ท ี ขํา 5 ะ ต า ม ข้า แด แต่ า กั น ท ี จะ น ํ า ไป ๓ ร ดิ ย

ต่า แห น ่ ง

 

 

 

 

) ส ํ า น ั ก [ต ่ า น ศุ ล ภา ก ร

 

 

 

ห น ึ ่ ง แส น ห ้ า ห ม ื ่ น ส อ ง ผัน ส า ม ร ้ อ ย นี่ ส ิ บ แป ด ม า ท ถ้วน .

. เ จ ้ า พ น ั ค ง า น ค า ร เง ิ น แต ะ บ ั ญ ชี ,

 

 

 ";

            Console.WriteLine("กำลังทดสอบการดึงข้อมูลทางการเงินจากข้อความ OCR...");
            Console.WriteLine("=================================================================");
            
            // ทดสอบการดึง Import Duty
            TestImportDuty(extractedText);
            
            // ทดสอบการดึง VAT
            TestVat(extractedText);
            
            // ทดสอบการดึง Total Amount
            TestTotalAmount(extractedText);
            
            Console.WriteLine("\nการทดสอบเสร็จสิ้น");
        }
        
        static void TestImportDuty(string extractedText)
        {
            Console.WriteLine("\n=== ทดสอบการดึง Import Duty ===");
            
            // Pattern เดิม
            var importDutyMatch = Regex.Match(extractedText, @"อากร\w*ขาเข้า\s*(\d+\.?\d*)");
            if (importDutyMatch.Success)
            {
                Console.WriteLine($"✅ Pattern เดิม พบ Import Duty: {importDutyMatch.Groups[1].Value}");
            }
            else
            {
                Console.WriteLine("❌ Pattern เดิม ไม่พบ Import Duty");
                
                // Pattern ใหม่
                importDutyMatch = Regex.Match(extractedText, @"ล\s*ํ\s*า\s*อ\s*า\s*ก\s*ร\s*ษา\s*ข้า\s*(\d+\.?\d*)");
                if (importDutyMatch.Success)
                {
                    Console.WriteLine($"✅ Pattern ใหม่ พบ Import Duty: {importDutyMatch.Groups[1].Value}");
                }
                else
                {
                    Console.WriteLine("❌ Pattern ใหม่ ไม่พบ Import Duty");
                }
            }
        }
        
        static void TestVat(string extractedText)
        {
            Console.WriteLine("\n=== ทดสอบการดึง VAT ===");
            
            // Pattern เดิม
            var vatMatch = Regex.Match(extractedText, @"(ภาษีมูลค่าเพิ่ม|ค่าภาษียุลค่าเพิ่ม)\s*(\d+\.?\d*)");
            if (vatMatch.Success)
            {
                Console.WriteLine($"✅ Pattern เดิม พบ VAT: {vatMatch.Groups[2].Value}");
            }
            else
            {
                Console.WriteLine("❌ Pattern เดิม ไม่พบ VAT");
                
                // Pattern ใหม่
                vatMatch = Regex.Match(extractedText, @"ค\s*่\s*า\s*ภา\s*ษี\s*ย\s*ุ\s*ล\s*ค\s*่\s*า\s*เพ\s*ิ\s*่\s*ม\s*(\d+\.?\d*)");
                if (vatMatch.Success)
                {
                    Console.WriteLine($"✅ Pattern ใหม่ พบ VAT: {vatMatch.Groups[1].Value}");
                }
                else
                {
                    Console.WriteLine("❌ Pattern ใหม่ ไม่พบ VAT");
                }
            }
        }
        
        static void TestTotalAmount(string extractedText)
        {
            Console.WriteLine("\n=== ทดสอบการดึง Total Amount ===");
            
            // Pattern เดิม
            var totalMatch = Regex.Match(extractedText, @"(?:รวม|ยอดรวม|fuom).+?(\d+\.?\d*)");
            if (totalMatch.Success)
            {
                Console.WriteLine($"✅ Pattern เดิม พบ Total Amount: {totalMatch.Groups[1].Value}");
            }
            else
            {
                Console.WriteLine("❌ Pattern เดิม ไม่พบ Total Amount");
                
                // Pattern ใหม่
                totalMatch = Regex.Match(extractedText, @"fuom\|\s*(\d+)\s*§");
                if (totalMatch.Success)
                {
                    Console.WriteLine($"✅ Pattern ใหม่ พบ Total Amount: {totalMatch.Groups[1].Value}");
                }
                else
                {
                    Console.WriteLine("❌ Pattern ใหม่ ไม่พบ Total Amount");
                }
            }
        }
    }
}
