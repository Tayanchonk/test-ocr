#!/usr/bin/env python3
"""
Simple test script to verify the OCR API functionality
"""
import requests
import json
import time
import os

def test_ocr_api():
    base_url = "http://localhost:5053"
    
    # Create a simple test image (we'll simulate this with a small PNG)
    test_file_path = "/tmp/test_image.txt"
    with open(test_file_path, 'w') as f:
        f.write("This is a test document\nDocument Number: 1234567\nTotal Amount: 152.00")
    
    print("Testing OCR API...")
    
    # Test upload endpoint
    try:
        with open(test_file_path, 'rb') as f:
            files = {'file': ('test.txt', f, 'text/plain')}
            response = requests.post(f"{base_url}/api/ocr/upload-financial", files=files)
            
        if response.status_code == 200:
            result = response.json()
            process_id = result.get('id')
            print(f"✓ Upload successful! Process ID: {process_id}")
            
            # Test status endpoint
            time.sleep(2)  # Wait for processing
            status_response = requests.get(f"{base_url}/api/ocr/status/{process_id}")
            
            if status_response.status_code == 200:
                status_result = status_response.json()
                print(f"✓ Status check successful!")
                print(f"  Status: {status_result.get('status')}")
                print(f"  Errors: {status_result.get('errors', [])}")
                
                if not status_result.get('errors'):
                    print("✓ No stream errors detected!")
                else:
                    print(f"✗ Errors found: {status_result.get('errors')}")
            else:
                print(f"✗ Status check failed: {status_response.status_code}")
        else:
            print(f"✗ Upload failed: {response.status_code} - {response.text}")
            
    except Exception as e:
        print(f"✗ Test failed with exception: {e}")
    
    # Cleanup
    if os.path.exists(test_file_path):
        os.remove(test_file_path)

if __name__ == "__main__":
    test_ocr_api()