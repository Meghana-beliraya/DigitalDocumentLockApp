using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDocumentLockCommon.Models
{
    [Table("Document")]
    public class Document
    {
        [Key]
        [Column("document_id")]
        public int DocumentId { get; set; }

        [Required]
        [Column("file_name")]
        public string FileName { get; set; }

        [Required]
        [Column("file_path")]
        public string FilePath { get; set; }

        [Required]
        [Column("Password")] // Capitalized to match the DB column
        public string Password { get; set; }  // Consider encrypting later

        [Required]
        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        //[Required]
        //[Column("expiry_date")]
        //public DateTime ExpiryDate { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public long FileSize { get; set; }        // file_size in bytes
        public string FileType { get; set; } = ""; // e.g., "application/pdf"
        public bool DeleteInd { get; set; } = false;
    }
}
