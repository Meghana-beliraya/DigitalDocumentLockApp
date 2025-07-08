public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalDocuments { get; set; }
    public int DocumentsUploadedToday { get; set; }
    public List<ActivityLogDto> ActivityLogs { get; set; }
}