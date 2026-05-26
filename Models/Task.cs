using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFlowAPI.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public TaskStatus Status { get; set; } = TaskStatus.ÀFaire;
        
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public DateTime? DueDate { get; set; }
        
        public string Comments { get; set; } = "[]";
    }
    
    public enum TaskStatus
    {
        ÀFaire,
        EnCours,
        Terminé
    }
}