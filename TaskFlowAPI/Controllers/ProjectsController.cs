using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Dto;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize] // Toutes les routes de ce contrôleur nécessitent un token valide
    public class ProjectsController : ControllerBase
    {
        private readonly ApiContext _context;

        public ProjectsController(ApiContext context)
        {
            _context = context;
        }

        // Récupération de tous les projets
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.Projects
                .Select(p => new { p.Id, p.Name, p.Description, p.CreatedAt })
                .ToListAsync();

            return Ok(projects);
        }

        // Création d'un nouveau projet
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // On renvoie un 201 avec l'URL du projet créé dans le header Location
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        // Récupération d'un projet par son identifiant
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound(new { message = $"Projet {id} introuvable." });

            return Ok(project);
        }

        // Mise à jour d'un projet existant
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound(new { message = $"Projet {id} introuvable." });

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(project);
        }

        // Suppression d'un projet
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound(new { message = $"Projet {id} introuvable." });

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
