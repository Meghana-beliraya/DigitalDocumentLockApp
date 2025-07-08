using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

public class DocumentUploadDto
{
    public IFormFile File { get; set; }  // lowercase 'file' to match form key

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    //[JsonPropertyName("expiry_date")]
    //public DateTime ExpiryDate { get; set; }
}
