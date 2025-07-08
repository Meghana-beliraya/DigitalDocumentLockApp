

using DigitalDocumentLockCommon.Models;

namespace DigitalDocumentLockCommon.Dtos
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoginResponseDto? Data { get; set; }
    }
}
