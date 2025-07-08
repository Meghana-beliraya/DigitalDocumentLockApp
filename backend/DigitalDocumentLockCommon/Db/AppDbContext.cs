using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Models; 

namespace DigitalDocumentLockCommon.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }  // Example DbSet
        public DbSet<Document> Document { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    }
}
