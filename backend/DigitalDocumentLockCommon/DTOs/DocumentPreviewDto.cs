namespace DigitalDocumentLockCommom.DTOs
{
    public class DocumentPreviewDto
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string? ErrorMessage { get; set; } // For handling errors in repo gracefully
        public int? StatusCode { get; set; }      // Optional, for returning 400, 404, etc.
    }
}
