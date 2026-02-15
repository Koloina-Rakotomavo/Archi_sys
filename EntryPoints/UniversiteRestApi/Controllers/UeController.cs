using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases;

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
        var useCase = new CreateUeUseCase(repositoryFactory.UeRepository());
        var created = await useCase.ExecuteAsync(request.NumeroUe, request.Intitule);
        return Ok(created);
    }
}
