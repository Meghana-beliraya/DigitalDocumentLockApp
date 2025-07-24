// File: Models/DashboardData.cs
namespace DigitalDocumentLockCommom.DTOs
{
    public class DashboardData
    {
        public int total_file { get; set; }
        public int no_of_docs { get; set; }
        public int no_of_pdf { get; set; }
        public List<string> RecentActivities { get; set; } 
    }

}