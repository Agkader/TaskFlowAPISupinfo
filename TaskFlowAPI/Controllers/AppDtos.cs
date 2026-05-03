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
    }

    // DTOs pour les tâches
    // Le statut peut valoir "Todo", "InProgress" ou "Done"

    public class TaskDto
    {
        public string  Title       { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Status      { get; set; }
        public int     ProjectId   { get; set; }
    }
}
