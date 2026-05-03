using TaskFlowAPI.Models;

namespace TaskFlowAPI.Dto
{
    // DTOs pour l'authentification

    public class RegisterDto
    {
        public string Name     { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTOs pour les projets

    public class ProjectDto
    {
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int     UserId      { get; set; }
    }

    // DTOs pour les tâches
    // Status accepte : ÀFaire, EnCours, Terminé

    public class TaskDto
    {
        public string      Title     { get; set; } = string.Empty;
        public TaskStatus? Status    { get; set; }
        public DateTime?   DueDate   { get; set; }
        public string?     Comments  { get; set; }
        public int         ProjectId { get; set; }
    }
}
