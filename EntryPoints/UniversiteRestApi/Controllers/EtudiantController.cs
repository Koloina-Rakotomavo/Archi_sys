using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;

namespace UniversiteRestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    public record CreateEtudiantRequest(string NumEtud, string Nom, string Prenom, string Email);

    [HttpGet]
    public async Task<IActionResult> FindAll()
    {
        if (!HasAuthorizedRole())
            return Forbid();

        var etudiants = await repositoryFactory.EtudiantRepository().FindAllAsync();
        return Ok(etudiants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEtudiantRequest request)
    {
        if (!HasAuthorizedRole())
            return Forbid();

        var useCase = new CreateEtudiantUseCase(repositoryFactory);
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        if (!useCase.IsAuthorized(role))
            return Forbid();

        try
        {
            var created = await useCase.ExecuteAsync(request.NumEtud, request.Nom, request.Prenom, request.Email);
            return CreatedAtAction(nameof(FindAll), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private bool HasAuthorizedRole()
    {
        var roleClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToHashSet();
        return roleClaims.Contains(Roles.Administrateur) ||
               roleClaims.Contains(Roles.Responsable) ||
               roleClaims.Contains(Roles.Scolarite);
    }
}
