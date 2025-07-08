public class UserSummaryDto
{
	public int UserId { get; set; }
	public string FirstName { get; set; } = "";
	public string LastName { get; set; } = "";
	public string Email { get; set; } = "";
	public bool IsActive { get; set; }
	public DateTime? LastLogin { get; set; }
	public int TotalDocumentsUploaded { get; set; }
}
