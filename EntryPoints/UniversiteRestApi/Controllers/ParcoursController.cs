using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.ParcoursUseCases.Create;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteRestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParcoursController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    public record CreateParcoursRequest(string NomParcours, int AnneeFormation);
    public record AddEtudiantsRequest(long[] IdEtudiants);
    public record AddUesRequest(long[] IdUes);

    [HttpGet]
    public async Task<IActionResult> FindAll()
    {
        var result = await repositoryFactory.ParcoursRepository().FindAllAsync();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrateur + "," + Roles.Responsable + "," + Roles.Scolarite)]
    public async Task<IActionResult> Create([FromBody] CreateParcoursRequest request)
    {
        var useCase = new CreateParcoursUseCase(repositoryFactory);
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        if (!useCase.IsAuthorized(role))
            return Forbid();

        try
        {
            var created = await useCase.ExecuteAsync(request.NomParcours, request.AnneeFormation);
            return CreatedAtAction(nameof(FindAll), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{idParcours:long}/etudiants")]
    [Authorize(Roles = Roles.Administrateur + "," + Roles.Responsable + "," + Roles.Scolarite)]
    public async Task<IActionResult> AddEtudiants(long idParcours, [FromBody] AddEtudiantsRequest request)
    {
        var useCase = new AddEtudiantDansParcoursUseCase(repositoryFactory);
        var updated = await useCase.ExecuteAsync(idParcours, request.IdEtudiants);
        return Ok(updated);
    }

    [HttpPost("{idParcours:long}/ues")]
    [Authorize(Roles = Roles.Administrateur + "," + Roles.Responsable + "," + Roles.Scolarite)]
    public async Task<IActionResult> AddUes(long idParcours, [FromBody] AddUesRequest request)
    {
        var useCase = new AddUeDansParcoursUseCase(repositoryFactory);
        var updated = await useCase.ExecuteAsync(idParcours, request.IdUes);
        return Ok(updated);
    }
}
