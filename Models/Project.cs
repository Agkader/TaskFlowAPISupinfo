using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFlowAPI.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}