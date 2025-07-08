namespace YourNamespace.Models
{
    public class UpdateProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}