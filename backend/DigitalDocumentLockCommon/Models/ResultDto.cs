namespace DigitalDocumentLockCommon.Models
{
    public class ResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
        public object? Data { get; set; }
        public int StatusCode { get; set; } = 400;
    }
}
