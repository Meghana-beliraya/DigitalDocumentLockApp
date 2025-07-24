using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
namespace DigitalDocumentLockCommom.DTOs
{
    public class DocumentUploadDto
    {
        public IFormFile File { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        //[JsonPropertyName("expiry_date")]
        //public DateTime ExpiryDate { get; set; }
    }
}
