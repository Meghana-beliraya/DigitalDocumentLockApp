namespace DigitalDocumentLockCommom.DTOs
{
	public class DocumentDisplayDto
	{
        public int DocumentId { get; set; }
		public int UserId { get; set; }
        public string FileName { get; set; }
		public string UploadedBy { get; set; }
		public DateTime UploadedAt { get; set; }
        //public DateTime? ExpiryDate { get; set; }
		public long FileSize { get; set; }
		public string FileType { get; set; }
	}
}

