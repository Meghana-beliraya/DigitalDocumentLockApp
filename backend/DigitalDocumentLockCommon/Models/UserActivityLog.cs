using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDocumentLockCommon.Models;

[Table("user_activity_log")]
public class UserActivityLog
{
    [Key]
    [Column("log_id")]
    public int LogId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;  // Navigation property

    [Required]
    [Column("activity")]
    public string Activity { get; set; } = null!;

    [Required]
    [Column("activity_date")]
    public DateTime ActivityDate { get; set; }
}
