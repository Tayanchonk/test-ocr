using System;
using System.Text.RegularExpressions;

namespace DebugPatterns
{
    class Program
    {
        static void Main(string[] args)
        {
            // ข้อมูลดิบจาก OCR ที่ได้มา (ตรงตามที่ได้มา)
            string extractedText = @"6103210097370000010.

 

เ " เก ค า ร
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

            Console.WriteLine("=== DEBUG: ทดสอบ patterns ทั้งหมด ===");
            Console.WriteLine();
            
            // ลองทดสอบ patterns ต่างๆ
            Console.WriteLine("=== ทดสอบ Import Duty Patterns ===");
            TestImportDutyPatterns(extractedText);
            
            Console.WriteLine("\n=== ทดสอบ VAT Patterns ===");
            TestVatPatterns(extractedText);
            
            Console.WriteLine("\n=== ทดสอบ Total Amount Patterns ===");
            TestTotalPatterns(extractedText);
            
            // แสดงข้อความส่วนที่เกี่ยวข้อง
            Console.WriteLine("\n=== ข้อความที่เกี่ยวข้อง ===");
            var lines = extractedText.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("ล ํ า อ า ก ร") || 
                    line.Contains("ค ่ า ภา ษี") || 
                    line.Contains("fuom") ||
                    line.Contains("86061") ||
                    line.Contains("6626700") ||
                    line.Contains("15532600"))
                {
                    Console.WriteLine($"พบบรรทัด: '{line.Trim()}'");
                }
            }
        }
        
        static void TestImportDutyPatterns(string text)
        {
            // Pattern เดิม
            var pattern1 = @"อากร\w*ขาเข้า\s*(\d+\.?\d*)";
            var match1 = Regex.Match(text, pattern1);
            Console.WriteLine($"Pattern 1: {pattern1}");
            Console.WriteLine($"Result: {(match1.Success ? $"พบ: {match1.Groups[1].Value}" : "ไม่พบ")}");
            
            // Pattern ใหม่
            var pattern2 = @"ล\s*ํ\s*า\s*อ\s*า\s*ก\s*ร\s*ษา\s*ข้า\s*(\d+\.?\d*)";
            var match2 = Regex.Match(text, pattern2);
            Console.WriteLine($"Pattern 2: {pattern2}");
            Console.WriteLine($"Result: {(match2.Success ? $"พบ: {match2.Groups[1].Value}" : "ไม่พบ")}");
            
            // Pattern ที่ง่ายกว่า
            var pattern3 = @"ล\s+ํ\s+า\s+อ\s+า\s+ก\s+ร\s+ษา\s+ข้า\s+(\d+\.?\d*)";
            var match3 = Regex.Match(text, pattern3);
            Console.WriteLine($"Pattern 3: {pattern3}");
            Console.WriteLine($"Result: {(match3.Success ? $"พบ: {match3.Groups[1].Value}" : "ไม่พบ")}");
        }
        
        static void TestVatPatterns(string text)
        {
            // Pattern เดิม
            var pattern1 = @"(ภาษีมูลค่าเพิ่ม|ค่าภาษียุลค่าเพิ่ม)\s*(\d+\.?\d*)";
            var match1 = Regex.Match(text, pattern1);
            Console.WriteLine($"Pattern 1: {pattern1}");
            Console.WriteLine($"Result: {(match1.Success ? $"พบ: {match1.Groups[2].Value}" : "ไม่พบ")}");
            
            // Pattern ใหม่
            var pattern2 = @"ค\s*่\s*า\s*ภา\s*ษี\s*ย\s*ุ\s*ล\s*ค\s*่\s*า\s*เพ\s*ิ\s*่\s*ม\s*(\d+\.?\d*)";
            var match2 = Regex.Match(text, pattern2);
            Console.WriteLine($"Pattern 2: {pattern2}");
            Console.WriteLine($"Result: {(match2.Success ? $"พบ: {match2.Groups[1].Value}" : "ไม่พบ")}");
            
            // Pattern ที่ง่ายกว่า
            var pattern3 = @"ค\s+่\s+า\s+ภา\s+ษี\s+ย\s+ุ\s+ล\s+ค\s+่\s+า\s+เพ\s+ิ\s+่\s+ม\s+(\d+)";
            var match3 = Regex.Match(text, pattern3);
            Console.WriteLine($"Pattern 3: {pattern3}");
            Console.WriteLine($"Result: {(match3.Success ? $"พบ: {match3.Groups[1].Value}" : "ไม่พบ")}");
        }
        
        static void TestTotalPatterns(string text)
        {
            // Pattern เดิม
            var pattern1 = @"(?:รวม|ยอดรวม|fuom).+?(\d+\.?\d*)";
            var match1 = Regex.Match(text, pattern1);
            Console.WriteLine($"Pattern 1: {pattern1}");
            Console.WriteLine($"Result: {(match1.Success ? $"พบ: {match1.Groups[1].Value}" : "ไม่พบ")}");
            
            // Pattern ใหม่
            var pattern2 = @"fuom\|\s*(\d+)\s*§";
            var match2 = Regex.Match(text, pattern2);
            Console.WriteLine($"Pattern 2: {pattern2}");
            Console.WriteLine($"Result: {(match2.Success ? $"พบ: {match2.Groups[1].Value}" : "ไม่พบ")}");
            
            // Pattern อื่น
            var pattern3 = @"fuom\|\s*(\d+)";
            var match3 = Regex.Match(text, pattern3);
            Console.WriteLine($"Pattern 3: {pattern3}");
            Console.WriteLine($"Result: {(match3.Success ? $"พบ: {match3.Groups[1].Value}" : "ไม่พบ")}");
        }
    }
}
