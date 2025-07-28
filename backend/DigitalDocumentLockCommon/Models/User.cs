using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDocumentLockCommon.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("user_id")] // Updated to match DB
    public int Id { get; set; }

    [Required]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string? LastName { get; set; }

    [Required]
    [Column("Email")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("Password")] // Capitalized to match DB
    public string Password { get; set; } = null!;

    [Column("profile_image_url")]
    public string? ProfileImageUrl { get; set; }


    [Column("isAdmin")] //  Updated to match DB
    public bool IsAdmin { get; set; }

    [Column("isActive")] // Updated to match DB
    public bool IsActive { get; set; } = true;

 

}
