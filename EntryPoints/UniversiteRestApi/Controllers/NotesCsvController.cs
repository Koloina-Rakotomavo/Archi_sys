using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.UseCases.NoteUseCases.Get;
using UniversiteDomain.UseCases.NoteUseCases.Update;

namespace UniversiteRestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Scolarite)]
public class NotesCsvController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    [HttpGet("template/{idUe:long}")]
    public async Task<IActionResult> ExportTemplate(long idUe)
    {
        var useCase = new GetUeNotesTemplateUseCase(repositoryFactory);
        var rows = await useCase.ExecuteAsync(idUe);

        await using var memoryStream = new MemoryStream();
        await using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                   {
                       Delimiter = ";"
                   }))
        {
            await csv.WriteRecordsAsync(rows);
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "text/csv", $"notes-ue-{idUe}.csv");
    }

    [HttpPost("import/{idUe:long}")]
    public async Task<IActionResult> Import(long idUe, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Fichier CSV manquant.");

        List<UeNoteCsvRow> rows;
        await using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = ";",
                   HeaderValidated = null,
                   MissingFieldFound = null
               }))
        {
            rows = csv.GetRecords<UeNoteCsvRow>().ToList();
        }

        var useCase = new ImportUeNotesUseCase(repositoryFactory);
        try
        {
            await useCase.ExecuteAsync(idUe, rows);
            return Ok(new { message = "Import des notes termine." });
        }
        catch (CsvImportValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}
