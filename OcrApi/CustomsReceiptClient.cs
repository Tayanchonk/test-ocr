using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomsReceiptClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Customs Receipt OCR Parser Client");
            Console.WriteLine("=================================");
            
            // กำหนด URL ของ API
            string apiUrl = "http://localhost:5000/api/ocr/custom-ocr-by-copilot";
            
            // ตัวอย่าง raw text จาก OCR
            string rawText = @"A ) o 7 ~
| ร ใบ เส ร ็ จ ร ั บ เง ิ น
ห ผ่ ก ศก ,. 122
Hawb ฟ ผด. 1746172551 |
ชื ่ อ ย า น พ า ห น ะ 07556 ก ร ม ศุ ด ก า ก ร
เล ข ป ร ะ จ ํ า ต ั ว ผู ้ เส ี ย ภา ษี อ า ก ร 0105533022910/000000 ง ั น ท ี ่ น ํ า เข ้ า / ส ่ ง อ อ ก 25-01-2566 
งื ่ ล ผู้ น ํ า ขอ ง เข ้ า / ผ ู ้ ส ่ ง ขอ ง อ อ ก DHL EXPRESS (โห ล 1 ) LTD.
เล ข ท ี ่ ชํา ร ะ อ า ก ร / ว ั น เด ื อ น ป ี 1818-061169/27-01-66(03/03)
A025-X660100256 (1193)
ค ่ า อ า ก ร ขา เข ้ า - 937.63
ค ํ า ภา ษี ม ู ล ค ่ า เพ ิ ่ ม 65.64
1.00
รวมเงินทั้งสิ้น 1,004.47
น ว น เง ิ น ต ั ว อ ั ก ษ ร หนึ่งพันสี่บาทสี่สิบเจ็ดสตางค์
ล ง ชื อ ผู ้ ร ั บ เง ิ น ว ั ชน ี
( เก ต จ นา )";
            
            try
            {
                // สร้าง HttpClient และส่งข้อมูลไปที่ API
                using (HttpClient client = new HttpClient())
                {
                    // สร้าง request object
                    var requestObject = new { Text = rawText };
                    
                    // แปลง object เป็น JSON
                    string jsonRequest = JsonSerializer.Serialize(requestObject);
                    
                    // สร้าง StringContent สำหรับส่งไปกับ request
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    
                    Console.WriteLine("Sending request to API...");
                    
                    // ส่ง POST request ไปที่ API
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    
                    // อ่านข้อมูล response
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    
                    // ตรวจสอบว่า request สำเร็จหรือไม่
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("API response received successfully!");
                        
                        // แปลง JSON response เป็น object ที่อ่านง่าย (indented)
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        var jsonFormatted = JsonSerializer.Serialize(
                            JsonSerializer.Deserialize<dynamic>(jsonResponse), options);
                            
                        // แสดงผลลัพธ์
                        Console.WriteLine("\nParsed Customs Receipt Data:");
                        Console.WriteLine("==========================");
                        Console.WriteLine(jsonFormatted);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        Console.WriteLine(jsonResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
