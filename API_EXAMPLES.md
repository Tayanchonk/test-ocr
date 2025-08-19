# API Usage Examples

This document provides practical examples of how to use the OCR API for processing receipts and financial documents.

## Base URL

```
Development: http://localhost:5053
Production: https://your-api-domain.com
```

## Authentication

Currently, no authentication is required (for development). In production, implement appropriate authentication mechanisms.

## Example 1: Upload Financial Document

### Request
```bash
curl -X POST "http://localhost:5053/api/ocr/upload-financial" \
     -H "Content-Type: multipart/form-data" \
     -F "file=@receipt.jpg" \
     -F "language=tha+eng"
```

### Response
```json
{
  "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "message": "Financial document uploaded and processing started",
  "status": 0
}
```

## Example 2: Check Processing Status

### Request
```bash
curl -X GET "http://localhost:5053/api/ocr/status/a1b2c3d4-e5f6-7890-1234-567890abcdef"
```

### Response (Processing)
```json
{
  "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "fileName": "receipt.jpg",
  "fileType": "image/jpeg",
  "status": 0,
  "extractedText": null,
  "financialData": null,
  "processedAt": "2024-01-20T10:30:00Z",
  "errors": [],
  "confidenceScore": 0
}
```

### Response (Completed)
```json
{
  "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "fileName": "receipt.jpg",
  "fileType": "image/jpeg",
  "status": 1,
  "extractedText": "ใบเสร็จรับเงิน\nเลขที่ 0431338\nชื่อผู้เกี่ยวข้อง: MRS. VICHUDA SUBSIRI\nหมายเลขอ้างอิง: 700352/24-07-51\nจำนวนเงิน: 322.00\nวันที่: 24-07-51",
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
      },
      {
        "description": "Extracted expense item", 
        "amount": 162.00,
        "quantity": 1
      },
      {
        "description": "Extracted expense item",
        "amount": 50.00,
        "quantity": 1
      }
    ],
    "totalAmount": 322.00,
    "currency": "THB"
  },
  "processedAt": "2024-01-20T10:31:45Z",
  "errors": [],
  "confidenceScore": 87.5
}
```

## Example 3: Download Results as JSON

### Request
```bash
curl -X GET "http://localhost:5053/api/ocr/download/a1b2c3d4-e5f6-7890-1234-567890abcdef" \
     --output result.json
```

This will download the complete result as a JSON file named `result.json`.

## Example 4: General OCR (Non-Financial Documents)

### Request
```bash
curl -X POST "http://localhost:5053/api/ocr/upload" \
     -H "Content-Type: multipart/form-data" \
     -F "file=@document.png" \
     -F "language=eng"
```

### Response
```json
{
  "id": "b2c3d4e5-f6g7-8901-2345-678901bcdefg",
  "message": "File uploaded and processing started",
  "status": 0
}
```

## Example 5: List All Results

### Request
```bash
curl -X GET "http://localhost:5053/api/ocr/results"
```

### Response
```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "fileName": "receipt.jpg",
    "fileType": "image/jpeg",
    "status": 1,
    "extractedText": "...",
    "financialData": { ... },
    "processedAt": "2024-01-20T10:31:45Z",
    "errors": [],
    "confidenceScore": 87.5
  },
  {
    "id": "b2c3d4e5-f6g7-8901-2345-678901bcdefg",
    "fileName": "document.png",
    "fileType": "image/png", 
    "status": 1,
    "extractedText": "Sample text extracted from document...",
    "financialData": null,
    "processedAt": "2024-01-20T10:25:30Z",
    "errors": [],
    "confidenceScore": 92.3
  }
]
```

## Status Codes

- `0` - Processing: File is being processed
- `1` - Completed: Processing completed successfully  
- `2` - Failed: Processing failed (check errors array)

## Error Handling

### File Too Large
```json
{
  "message": "File size exceeds 10MB limit"
}
```

### Invalid File Type
```json
{
  "message": "Invalid file type. Only JPEG, PNG, and PDF files are allowed."
}
```

### Processing Error
```json
{
  "id": "c3d4e5f6-g7h8-9012-3456-789012cdefgh",
  "fileName": "corrupted.jpg",
  "fileType": "image/jpeg",
  "status": 2,
  "extractedText": null,
  "financialData": null,
  "processedAt": "2024-01-20T10:35:00Z",
  "errors": ["Failed to extract text from image: Image data is corrupted"],
  "confidenceScore": 0
}
```

## Tips for Better Results

1. **Image Quality**: Use high-resolution, well-lit images
2. **File Format**: JPEG and PNG work best for photos; PDF for scanned documents
3. **Language**: Specify correct language codes (e.g., "tha+eng" for Thai and English)
4. **Document Orientation**: Ensure text is right-side up
5. **Clean Background**: Remove shadows and ensure good contrast

## Language Codes

- `eng` - English
- `tha` - Thai  
- `tha+eng` - Thai and English (recommended for Thai receipts)
- `chi_sim` - Chinese Simplified
- `jpn` - Japanese
- `fra` - French
- `deu` - German
- `spa` - Spanish

## Rate Limits

- Maximum file size: 10MB
- Concurrent processing: 10 files
- Timeout: 5 minutes per file