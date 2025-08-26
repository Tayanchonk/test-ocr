using System;
using System.Text.RegularExpressions;

namespace TestDeclarantName
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

            Console.WriteLine("กำลังทดสอบการดึง declarantName จากข้อความ OCR...");
            Console.WriteLine("=================================================================");
            
            string normalizedText = NormalizeText(extractedText);
            
            // ลองใช้ pattern ใหม่ที่เราเขียน
            var waPatternMatch = Regex.Match(normalizedText, @"ชื\s*่\s*อ\s*ผู\s*้\s*น\s*ํ\s*า\s*ขอ\s*ง\s*เข\s*้\s*า\s*/\s*ผ\s*ู\s*้\s*ส\s*่\s*ง\s*ขอ\s*ง\s*อ\s*อ\s*ก\s+WA\s+(.*?)(?=\n|$)");
            if (waPatternMatch.Success)
            {
                string declarantName = waPatternMatch.Groups[1].Value.Trim();
                // ลบช่องว่างทั้งหมดระหว่างตัวอักษร
                declarantName = Regex.Replace(declarantName, @"\s+", "");
                Console.WriteLine($"✅ พบ declarantName จากรูปแบบ WA: '{declarantName}'");
            }
            else
            {
                Console.WriteLine("❌ ไม่พบ declarantName จากรูปแบบ WA แบบแรก");
                
                // ลองใช้ pattern ที่มีช่องว่างมากขึ้น
                var waPatternMatchSpaced = Regex.Match(normalizedText, @"ชื\s+่\s+อ\s+ผู\s+้\s+น\s+ํ\s+า\s+ขอ\s+ง\s+เข\s+้\s+า\s+/\s+ผ\s+ู\s+้\s+ส\s+่\s+ง\s+ขอ\s+ง\s+อ\s+อ\s+ก\s+WA\s+(.*?)(?=\n|$)");
                if (waPatternMatchSpaced.Success)
                {
                    string declarantName = waPatternMatchSpaced.Groups[1].Value.Trim();
                    // ลบช่องว่างทั้งหมดระหว่างตัวอักษร
                    declarantName = Regex.Replace(declarantName, @"\s+", "");
                    Console.WriteLine($"✅ พบ declarantName จากรูปแบบ WA (ช่องว่างมาก): '{declarantName}'");
                }
                else
                {
                    Console.WriteLine("❌ ไม่พบ declarantName จากรูปแบบ WA ทั้งสองแบบ");
                }
            }
            
            Console.WriteLine("\n=== ข้อมูลดิบที่ normalize แล้ว ===");
            Console.WriteLine(normalizedText);
        }
        
        static string NormalizeText(string text)
        {
            // ลบข้อมูลการขึ้นบรรทัดใหม่ที่ซ้ำซ้อน
            text = Regex.Replace(text, @"\n\s*\n", "\n");
            text = Regex.Replace(text, @"\r\n", "\n");
            text = Regex.Replace(text, @"\r", "\n");
            
            // ลบช่องว่างที่ไม่จำเป็นในแต่ละบรรทัด
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }
            
            return string.Join("\n", lines).Trim();
        }
    }
}
