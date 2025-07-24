namespace DigitalDocumentLockCommom.DTOs
{
    public class DocumentDownloadDto
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }

        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
    }
}
