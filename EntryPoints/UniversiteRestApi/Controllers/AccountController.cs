using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Securite;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.SecurityUseCases.Get;

namespace UniversiteRestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IRepositoryFactory repositoryFactory, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            return BadRequest("Email et mot de passe obligatoires.");

        var findUser = new FindUniversiteUserByEmailUseCase(repositoryFactory);
        var checkPassword = new CheckPasswordUseCase(repositoryFactory);
        var isInRole = new IsInRoleUseCase(repositoryFactory);

        var user = await findUser.ExecuteAsync(login.Email);
        if (user is null)
            return Unauthorized("Email ou mot de passe invalide.");

        var passwordOk = await checkPassword.ExecuteAsync(login.Email, login.Password);
        if (!passwordOk)
            return Unauthorized("Email ou mot de passe invalide.");

        var roles = new List<string>();
        foreach (var role in new[] { Roles.Administrateur, Roles.Responsable, Roles.Scolarite, Roles.Etudiant, Roles.Enseignant })
        {
            if (await isInRole.ExecuteAsync(login.Email, role))
                roles.Add(role);
        }

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = BuildToken(user.Email, user.Id, roles, expiresAt);
        return Ok(new { token, expiresAt, roles });
    }

    private string BuildToken(string email, string userId, IEnumerable<string> roles, DateTime expiresAt)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "CHANGE_ME_PLEASE_FOR_PRODUCTION_SECRET_KEY";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "UniversiteApi";
        var jwtAudience = configuration["Jwt:Audience"] ?? "UniversiteFrontend";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
