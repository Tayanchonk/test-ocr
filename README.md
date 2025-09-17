# OCR API for Thai Receipt and Financial Document Processing

This repository contains a .NET 8 Web API application for OCR (Optical Character Recognition) processing of PDF and image files, with specialized features for extracting structured data from Thai receipts and financial documents.

## Recent Bug Fix: Stream Disposal Issue

### Problem
The API was encountering "Cannot access a closed Stream" errors when processing uploaded files. This occurred because:

1. The `ProcessFileAsync` method in `ProcessingService.cs` was starting a background task
2. The background task tried to read from the `IFormFile` stream
3. ASP.NET Core disposed the request resources (including the file stream) after the controller method returned
4. The background task would then fail when trying to access the closed stream

### Solution
The issue was fixed by modifying the `ProcessingService.ProcessFileAsync` method to:

1. **Read file data immediately**: Copy the file content to a byte array before starting the background task
2. **Pass byte array to background task**: Use the byte array for processing instead of the original IFormFile
3. **Proper error handling**: Add try-catch around file reading with meaningful error messages
4. **Store file metadata**: Capture file name and content type before background processing

### Code Changes
- Modified `OcrApi/Services/ProcessingService.cs`
- Added proper stream handling and error management
- Ensured file data is captured before ASP.NET Core disposes request resources

### Testing
The fix has been tested and verified to:
- ✅ Eliminate "Cannot access a closed Stream" errors
- ✅ Successfully read and process uploaded files
- ✅ Provide proper error handling for file reading failures
- ✅ Maintain all existing functionality

## API Endpoints

- `POST /api/ocr/upload` - General OCR text extraction
- `POST /api/ocr/upload-financial` - Financial document data extraction
- `GET /api/ocr/status/{id}` - Check processing status
- `GET /api/ocr/download/{id}` - Download results as JSON
- `GET /api/ocr/results` - List all processing results

## Usage

```bash
# Upload a Thai receipt for financial data extraction
curl -X POST "http://localhost:5000/api/ocr/upload-financial" \
     -F "file=@receipt.jpg" \
     -F "language=tha+eng"

# Check processing status
curl "http://localhost:5000/api/ocr/status/{id}"
```

## Build and Run

```bash
cd OcrApi
dotnet build
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger documentation at `/swagger`.