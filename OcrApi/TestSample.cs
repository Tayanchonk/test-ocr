using OcrApi.Models;
using OcrApi.Services;
using System.Text.Json;

namespace OcrApi
{
    // This is a simple test file to verify the OCR parsing
    public class TestSample
    {
        public static void RunTest()
        {
            // Sample OCR text provided by the user
            string ocrText = @"ใบ เส ร บ รบ เง น
—_— !
< ง ก ศก . 122

ก ร ม ศุ ล ก า ก ร ส
เล ข ป ร ะ จ ํ า ต ั ว ผู ้ เส ี ย ภา ษี อ า ก ร 013 503 R

A o ง
ซื ่ อ ผู ้ น ํ า ขอ ง เข ้ า / ผ ู ้ ส ่ ง ขอ ง อ อ ก

 

 

 

 

 

ง ' ส ื ่ ๕ - ว ั จ เล ื ด 9. รี]
เล ข ท ี ่ ใบ ขน ส ิ น ค ้ า ล 014-0650101021 (0121) เล ข ท ี ่ ชํา ร ะ อ า ก ร / ว ั น เด ื อ น ป ี 0152-084607/14-01-65
ได ้ ร ั บ เง ิ น ต า ม ร า ย ก า ร ข้ า ง ล ่ า ง น ี ้ ไว ้ แล ้ ว ที ่ ชํา ร ะ ต า ม ส ํ า แดง (บ า ท ) ท ี ่ ว า ง ป ร ะ ก ั น (บ า ท )
ต่า อ า ก ร ขา เข ้ า 168.00
ค ํ า ภา ษี ม ู ล ค ่ า เพ ิ ่ ม 40,975.00
41,143.00
ด ร ว ม เง ิ น ท ั ้ ง ส ิ ้ น (บ า ท ) 41,143.00

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 

จ ํ า น ว น เง ิ น ต ั ว อ ั ก ษ ร

 

 

 

 

 

 

 

 

 

 

 

 

 

 

 ";
            
            // Create logger for the parser
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<CustomsReceiptParser>();
            
            // Create parser
            var parser = new CustomsReceiptParser(logger);
            
            // Parse the OCR text
            var receipt = parser.Parse(ocrText);
            
            // Output the parsed receipt as JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            string json = JsonSerializer.Serialize(receipt, options);
            Console.WriteLine("Parsed Receipt:");
            Console.WriteLine(json);
        }
    }
}
