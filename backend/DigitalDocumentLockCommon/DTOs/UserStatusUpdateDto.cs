namespace DigitalDocumentLockCommom.DTOs
{
    public class UserStatusUpdateDto
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
    }
}
