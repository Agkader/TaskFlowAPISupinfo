using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskFlowAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        public UserRole Role { get; set; } = UserRole.User;
        
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
    
    public enum UserRole
    {
        User,
        Admin
    }
}