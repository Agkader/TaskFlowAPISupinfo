using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskFlowAPI.Data;
using TaskFlowAPI.Dto;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Gestion de l'authentification : inscription et connexion des utilisateurs.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class AuthController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApiContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Créer un nouveau compte utilisateur.
        /// </summary>
        /// <param name="dto">Informations d'inscription (nom, email, mot de passe).</param>
        /// <returns>L'utilisateur créé sans son mot de passe.</returns>
        /// <response code="201">Compte créé avec succès.</response>
        /// <response code="400">Un compte existe déjà avec cet email.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // On vérifie qu'aucun compte n'existe déjà avec cet email
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Un utilisateur avec cet email existe déjà." });

            var user = new User
            {
                Name         = dto.Name,
                Email        = dto.Email,
                PasswordHash = HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new
            {
                user.Id,
                user.Name,
                user.Email
            });
        }

        /// <summary>
        /// Se connecter et obtenir un token JWT.
        /// </summary>
        /// <param name="dto">Email et mot de passe.</param>
        /// <returns>Un token JWT à utiliser dans les requêtes suivantes.</returns>
        /// <response code="200">Connexion réussie, token retourné.</response>
        /// <response code="401">Email ou mot de passe incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Message volontairement vague pour ne pas indiquer si c'est l'email ou le mot de passe qui est faux
            if (user == null || user.PasswordHash != HashPassword(dto.Password))
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });

            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        // Hachage SHA-256 du mot de passe avant stockage en base
        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        // Construction du token JWT signé avec la clé secrète
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims embarqués dans le token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:            jwtSettings["Issuer"],
                audience:          jwtSettings["Audience"],
                claims:            claims,
                expires:           DateTime.UtcNow.AddHours(
                                       double.Parse(jwtSettings["ExpiresInHours"] ?? "24")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
