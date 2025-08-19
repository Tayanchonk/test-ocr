# OCR API - .NET Application for Receipt and Financial Document Processing

A .NET 8 Web API application for OCR (Optical Character Recognition) processing of PDF and image files, with specialized features for extracting data from Thai receipts and financial documents.

## Features

- **File Upload Support**: JPEG, PNG, and PDF files
- **General OCR**: Extract text from any uploaded document
- **Financial Document Processing**: Specialized extraction of:
  - Document numbers
  - Person names (MRS./MR./MS. patterns)
  - Reference numbers
  - Expense items and amounts
  - Document dates
  - Total amounts
- **Asynchronous Processing**: Non-blocking file processing
- **Swagger UI**: Interactive API documentation
- **Status Tracking**: Monitor processing progress
- **JSON Export**: Download results in structured JSON format

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **OCR Engine**: Tesseract.NET
- **Image Processing**: SixLabors.ImageSharp
- **PDF Processing**: PdfSharp
- **API Documentation**: Swashbuckle (Swagger)

## Prerequisites

- .NET 8.0 SDK
- Tesseract language data files (for Thai and English)

## Installation and Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd test-ocr/OcrApi
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Download Tesseract language data** (optional for enhanced accuracy):
   ```bash
   # Create tessdata directory
   mkdir tessdata
   
   # Download Thai and English language data files
   # You can download from: https://github.com/tesseract-ocr/tessdata
   # Place tha.traineddata and eng.traineddata in the tessdata folder
   ```

4. **Build the application**:
   ```bash
   dotnet build
   ```

5. **Run the application**:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7200` (HTTPS) or `http://localhost:5200` (HTTP).

## API Endpoints

### 1. General OCR Processing
- **POST** `/api/ocr/upload`
- Upload any image or PDF for text extraction
- Parameters:
  - `file`: The file to process (form-data)
  - `language`: OCR language (optional, default: "tha+eng")

### 2. Financial Document Processing
- **POST** `/api/ocr/upload-financial`
- Upload financial documents for structured data extraction
- Parameters:
  - `file`: The financial document to process (form-data)
  - `language`: OCR language (optional, default: "tha+eng")

### 3. Check Processing Status
- **GET** `/api/ocr/status/{id}`
- Check the status of a processing job
- Returns: Processing status and results (if completed)

### 4. Download Results
- **GET** `/api/ocr/download/{id}`
- Download processing results as JSON file
- Only available when processing is completed

### 5. List All Results
- **GET** `/api/ocr/results`
- Get all processing results

## Usage Examples

### Using Swagger UI

1. Navigate to the root URL (e.g., `https://localhost:7200`)
2. The Swagger UI will load automatically
3. Try the "Upload Financial Document" endpoint with a receipt image
4. Use the returned ID to check status and download results

### Using cURL

**Upload a financial document**:
```bash
curl -X POST "https://localhost:7200/api/ocr/upload-financial" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@receipt.jpg"
```

**Check processing status**:
```bash
curl -X GET "https://localhost:7200/api/ocr/status/{processing-id}"
```

**Download results**:
```bash
curl -X GET "https://localhost:7200/api/ocr/download/{processing-id}" \
  --output result.json
```

## Sample JSON Output

For the provided receipt image, the API extracts:

```json
{
  "id": "12345-67890-abcdef",
  "fileName": "receipt.jpg",
  "fileType": "image/jpeg",
  "status": "Completed",
  "extractedText": "...",
  "financialData": {
    "documentNumber": "0431338",
    "personName": "VICHUDA SUBSIRI",
    "referenceNumber": "700352/24-07-51",
    "documentDate": "2024-07-24T00:00:00",
    "expenseItems": [
      {
        "description": "Extracted expense item",
        "amount": 110.00,
        "quantity": 1
      }
    ],
    "totalAmount": 322.00,
    "currency": "THB"
  },
  "processedAt": "2024-01-20T10:30:00Z",
  "confidenceScore": 85.5,
  "errors": []
}
```

## File Limitations

- **Maximum file size**: 10MB
- **Supported formats**: JPEG, PNG, PDF
- **Processing timeout**: 5 minutes per file

## Error Handling

The API includes comprehensive error handling for:
- Invalid file types
- File size limits
- OCR processing failures
- Unclear or unreadable documents

Errors are returned in a structured format with appropriate HTTP status codes.

## Development Notes

- The application uses in-memory storage for processing results
- For production use, consider implementing persistent storage
- PDF OCR requires additional libraries for optimal performance
- Tesseract language data files improve accuracy for specific languages

## License

This project is part of a demonstration for OCR capabilities in .NET applications.