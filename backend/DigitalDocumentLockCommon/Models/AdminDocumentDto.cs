// Dtos/AdminDocumentDto.cs
namespace DigitalDocumentLockCommon.Dtos
{
    public class AdminDocumentDto
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        //public DateTime ExpiryDate { get; set; }
    }
}
