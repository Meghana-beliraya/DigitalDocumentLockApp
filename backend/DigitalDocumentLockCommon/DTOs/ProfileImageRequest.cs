using Microsoft.AspNetCore.Http;


namespace DigitalDocumentLockCommom.DTOs
{
    public class ProfileImageRequest
    {
        public IFormFile Image { get; set; }
    }
}