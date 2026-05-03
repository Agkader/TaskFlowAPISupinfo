using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Dto;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize] // Toutes les routes de ce contrôleur nécessitent un token valide
    public class TasksController : ControllerBase
    {
        private readonly ApiContext _context;

        public TasksController(ApiContext context)
        {
            _context = context;
        }

        // Récupération de toutes les tâches
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.Tasks
                .Select(t => new { t.Id, t.Title, t.Description, t.Status, t.ProjectId, t.CreatedAt })
                .ToListAsync();

            return Ok(tasks);
        }

        // Création d'une nouvelle tâche
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskDto dto)
        {
            // On s'assure que le projet rattaché à cette tâche existe bien
            if (!await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId))
                return BadRequest(new { message = $"Projet {dto.ProjectId} introuvable." });

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status ?? "Todo",
                ProjectId = dto.ProjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        // Récupération d'une tâche par son identifiant
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound(new { message = $"Tâche {id} introuvable." });

            return Ok(task);
        }

        // Mise à jour d'une tâche (titre, description, statut, projet)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound(new { message = $"Tâche {id} introuvable." });

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status ?? task.Status; // On garde l'ancien statut si non fourni
            task.ProjectId = dto.ProjectId;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // Suppression d'une tâche
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound(new { message = $"Tâche {id} introuvable." });

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
