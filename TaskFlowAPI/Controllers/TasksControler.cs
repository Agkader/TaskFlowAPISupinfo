using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Dto;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Gestion des tâches. Toutes les routes nécessitent un token JWT valide.
    /// </summary>
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

        /// <summary>
        /// Récupérer la liste de toutes les tâches.
        /// </summary>
        /// <returns>Liste des tâches avec leur identifiant, titre, statut, projet et date d'échéance.</returns>
        /// <response code="200">Liste retournée avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.Tasks
                .Select(t => new { t.Id, t.Title, t.Status, t.ProjectId, t.DueDate, t.Comments })
                .ToListAsync();

            return Ok(tasks);
        }

        /// <summary>
        /// Créer une nouvelle tâche.
        /// </summary>
        /// <param name="dto">Titre, statut, date d'échéance et identifiant du projet associé.</param>
        /// <returns>La tâche créée.</returns>
        /// <remarks>
        /// Le statut accepte les valeurs suivantes : <c>ÀFaire</c>, <c>EnCours</c>, <c>Terminé</c>.
        /// Si non renseigné, la tâche est créée avec le statut <c>ÀFaire</c> par défaut.
        /// </remarks>
        /// <response code="201">Tâche créée avec succès.</response>
        /// <response code="400">Le projet associé n'existe pas ou données invalides.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] TaskDto dto)
        {
            // On s'assure que le projet rattaché à cette tâche existe bien
            if (!await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId))
                return BadRequest(new { message = $"Projet {dto.ProjectId} introuvable." });

            var task = new TaskItem
            {
                Title     = dto.Title,
                Status    = dto.Status ?? TaskStatus.ÀFaire,
                DueDate   = dto.DueDate,
                Comments  = dto.Comments ?? "[]",
                ProjectId = dto.ProjectId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        /// <summary>
        /// Récupérer une tâche par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la tâche.</param>
        /// <returns>La tâche correspondante.</returns>
        /// <response code="200">Tâche trouvée.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucune tâche avec cet identifiant.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound(new { message = $"Tâche {id} introuvable." });

            return Ok(task);
        }

        /// <summary>
        /// Mettre à jour une tâche existante.
        /// </summary>
        /// <param name="id">Identifiant de la tâche à modifier.</param>
        /// <param name="dto">Nouvelles valeurs (titre, statut, date d'échéance).</param>
        /// <returns>La tâche mise à jour.</returns>
        /// <response code="200">Tâche mise à jour avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucune tâche avec cet identifiant.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound(new { message = $"Tâche {id} introuvable." });

            task.Title    = dto.Title;
            task.Status   = dto.Status ?? task.Status; // On garde l'ancien statut si non fourni
            task.DueDate  = dto.DueDate;
            task.Comments = dto.Comments ?? task.Comments;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        /// <summary>
        /// Supprimer une tâche.
        /// </summary>
        /// <param name="id">Identifiant de la tâche à supprimer.</param>
        /// <response code="204">Tâche supprimée avec succès.</response>
        /// <response code="401">Token manquant ou invalide.</response>
        /// <response code="404">Aucune tâche avec cet identifiant.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
