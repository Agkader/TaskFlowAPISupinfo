using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Models;
//
namespace TaskFlowAPI.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        //
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Assurer que l'email est unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Convertir les enums en string
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasConversion<string>();
                
            // Convertir les enums en string
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
    }
}