using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    public async Task<List<Note>> FindByUeAsync(long ueId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ueId);
        ArgumentNullException.ThrowIfNull(Context.Notes);
        return await Context.Notes.Where(n => n.UeId == ueId).ToListAsync();
    }

    public async Task UpsertManyAsync(List<Note> notes)
    {
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(Context.Notes);

        foreach (var note in notes)
        {
            var existing = await Context.Notes.FindAsync(note.EtudiantId, note.UeId);
            if (existing is null)
            {
                await Context.Notes.AddAsync(note);
            }
            else
            {
                existing.Valeur = note.Valeur;
            }
        }
    }

    public async Task DeleteManyAsync(List<(long EtudiantId, long UeId)> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(Context.Notes);

        foreach (var key in keys)
        {
            var existing = await Context.Notes.FindAsync(key.EtudiantId, key.UeId);
            if (existing is not null)
            {
                Context.Notes.Remove(existing);
            }
        }
    }

    public async Task<NotesFileDto> WriteAsync(List<LigneSaisieDto> lignes, string fileNameBase)
    {
        ArgumentNullException.ThrowIfNull(lignes);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNameBase);

        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };

        await using var ms = new MemoryStream();
        await using (var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, config))
        {
            await csv.WriteRecordsAsync(lignes);
            await writer.FlushAsync();
        }

        return new NotesFileDto
        {
            Content = ms.ToArray(),
            FileName = $"{fileNameBase}.csv",
            ContentType = "text/csv"
        };
    }

    public Task<List<LigneSaisieDto>> ReadAsync(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var ms = new MemoryStream(content);
        using var reader = new StreamReader(ms, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        var lignes = csv.GetRecords<LigneSaisieDto>().ToList();
        return Task.FromResult(lignes);
    }
}
