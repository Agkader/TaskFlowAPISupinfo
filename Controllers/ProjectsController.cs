using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlowAPI.Data;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ApiContext _context;

        public ProjectsController(ApiContext context)
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
            
            if (IsAdmin())
            {
                var projects = await _context.Projects
                    .Include(p => p.User)
                    .ToListAsync();
                return Ok(projects);
            }
            
            var userProjects = await _context.Projects
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .ToListAsync();
            
            return Ok(userProjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });
            
            if (!IsAdmin() && project.UserId != GetUserId())
                return Forbid();
            
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                UserId = GetUserId()
            };
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });
            
            if (!IsAdmin() && project.UserId != GetUserId())
                return Forbid();
            
            project.Name = request.Name;
            project.Description = request.Description;
            
            await _context.SaveChangesAsync();
            
            return Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return NotFound(new { message = "Project not found" });
            
            if (!IsAdmin() && project.UserId != GetUserId())
                return Forbid();
            
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }

    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}