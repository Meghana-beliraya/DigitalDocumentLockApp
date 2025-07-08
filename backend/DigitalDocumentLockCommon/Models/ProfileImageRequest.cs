using Microsoft.AspNetCore.Http;


namespace YourNamespace.Models
{
    public class ProfileImageRequest
    {
        public IFormFile Image { get; set; }
    }
}