using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Dto;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Gestion des projets. Toutes les routes nécessitent un token JWT valide.
    /// </summary>
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

        /// <summary>
        /// Récupérer la liste de tous les projets.
        /// </summary>
        /// <returns>Liste des projets avec leur identifiant, nom, description et date de création.</returns>
        /// <response code="200">Liste retournée avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.Projects
                .Select(p => new { p.Id, p.Name, p.Description, p.CreationDate, p.UserId })
                .ToListAsync();

            return Ok(projects);
        }

        /// <summary>
        /// Créer un nouveau projet.
        /// </summary>
        /// <param name="dto">Nom et description du projet.</param>
        /// <returns>Le projet créé.</returns>
        /// <response code="201">Projet créé avec succès.</response>
        /// <response code="400">Données invalides.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] ProjectDto dto)
        {
            var project = new Project
            {
                Name         = dto.Name,
                Description  = dto.Description,
                CreationDate = DateTime.Now,
                UserId       = dto.UserId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // On renvoie un 201 avec l'URL du projet créé dans le header Location
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        /// <summary>
        /// Récupérer un projet par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du projet.</param>
        /// <returns>Le projet correspondant.</returns>
        /// <response code="200">Projet trouvé.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucun projet avec cet identifiant.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound(new { message = $"Projet {id} introuvable." });

            return Ok(project);
        }

        /// <summary>
        /// Mettre à jour un projet existant.
        /// </summary>
        /// <param name="id">Identifiant du projet à modifier.</param>
        /// <param name="dto">Nouvelles valeurs (nom, description).</param>
        /// <returns>Le projet mis à jour.</returns>
        /// <response code="200">Projet mis à jour avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucun projet avec cet identifiant.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound(new { message = $"Projet {id} introuvable." });

            project.Name        = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(project);
        }

        /// <summary>
        /// Supprimer un projet.
        /// </summary>
        /// <param name="id">Identifiant du projet à supprimer.</param>
        /// <response code="204">Projet supprimé avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucun projet avec cet identifiant.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
