namespace DigitalDocumentLockCommom.DTOs
{
    public class ActivityLogDto
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
    }
}