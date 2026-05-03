using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using TaskFlowAPI.Data;
using Task = TaskFlowAPI.Models.Task;           
using TaskStatus = TaskFlowAPI.Models.TaskStatus;
namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApiContext _context;

        public TasksController(ApiContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private bool IsAdmin()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var query = _context.Tasks.Include(t => t.Project);
            
            if (IsAdmin())
            {
                return Ok(await query.ToListAsync());
            }
            
            var tasks = await query
                .Where(t => t.Project.UserId == userId)
                .ToListAsync();
            
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
                return NotFound(new { message = "Task not found" });
            
            if (!IsAdmin() && task.Project.UserId != GetUserId())
                return Forbid();
            
            // Désérialiser les commentaires
            var comments = JsonSerializer.Deserialize<List<string>>(task.Comments) ?? new List<string>();
            
            return Ok(new { task.Id, task.Title, task.Status, task.DueDate, task.ProjectId, Comments = comments });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                return BadRequest(new { message = "Project not found" });
            
            if (!IsAdmin() && project.UserId != GetUserId())
                return Forbid();
            
           
            var task = new Task
            {
                Title = request.Title,
                ProjectId = request.ProjectId,
                DueDate = request.DueDate,
                Comments = "[]"
            };
            
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
                return NotFound(new { message = "Task not found" });
            
            if (!IsAdmin() && task.Project.UserId != GetUserId())
                return Forbid();
            
            task.Title = request.Title;
            task.Status = request.Status;
            task.DueDate = request.DueDate;
            
            if (request.Comments != null)
                task.Comments = JsonSerializer.Serialize(request.Comments);
            
            await _context.SaveChangesAsync();
            
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
                return NotFound(new { message = "Task not found" });
            
            if (!IsAdmin() && task.Project.UserId != GetUserId())
                return Forbid();
            
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] string comment)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
                return NotFound(new { message = "Task not found" });
            
            if (!IsAdmin() && task.Project.UserId != GetUserId())
                return Forbid();
            
            var comments = JsonSerializer.Deserialize<List<string>>(task.Comments) ?? new List<string>();
            comments.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm}] {comment}");
            task.Comments = JsonSerializer.Serialize(comments);
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Comment added", comments });
        }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string>? Comments { get; set; }
    }
}