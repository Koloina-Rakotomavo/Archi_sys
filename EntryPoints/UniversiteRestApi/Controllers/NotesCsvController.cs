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

        var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
        {
            Delimiter = ";"
        };

        await using var memoryStream = new MemoryStream();
        await using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, csvConfig))
        {
            await csv.WriteRecordsAsync(rows);
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "text/csv", $"notes-ue-{idUe}.csv");
    }

    [HttpPost("import/{idUe:long}")]
    public async Task<IActionResult> Import(long idUe, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Fichier CSV manquant.");
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Le fichier doit etre au format .csv.");

        List<UeNoteCsvRow> rows;
        var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
        {
            Delimiter = ";",
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, csvConfig);
            rows = csv.GetRecords<UeNoteCsvRow>().ToList();
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new[] { $"CSV invalide: {ex.Message}" } });
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
