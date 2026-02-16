using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteRestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UeController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    public record CreateUeRequest(string NumeroUe, string Intitule);

    [HttpGet]
    public async Task<IActionResult> FindAll()
    {
        var result = await repositoryFactory.UeRepository().FindAllAsync();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrateur + "," + Roles.Responsable + "," + Roles.Scolarite)]
    public async Task<IActionResult> Create([FromBody] CreateUeRequest request)
    {
        var useCase = new CreateUeUseCase(repositoryFactory);
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        if (!useCase.IsAuthorized(role))
            return Forbid();

        try
        {
            var created = await useCase.ExecuteAsync(request.NumeroUe, request.Intitule);
            return CreatedAtAction(nameof(FindAll), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
