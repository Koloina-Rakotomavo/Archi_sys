using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Update;

public class ImportUeNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long idUe, List<UeNoteCsvRow> rows)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
            throw new CsvImportValidationException(new List<string> { "Le fichier CSV est vide." });

        var ueRepo = repositoryFactory.UeRepository();
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        var noteRepo = repositoryFactory.NoteRepository();

        var ue = await ueRepo.FindAsync(idUe) ?? throw new UeNotFoundException(idUe.ToString());
        var errors = new List<string>();
        var upserts = new List<(Etudiant etudiant, decimal? valeur)>();

        foreach (var row in rows)
        {
            if (!string.Equals(row.NumeroUe?.Trim(), ue.NumeroUe, StringComparison.OrdinalIgnoreCase))
                errors.Add($"UE invalide pour l'etudiant {row.NumEtud}: numero attendu '{ue.NumeroUe}'.");

            if (!string.IsNullOrWhiteSpace(row.IntituleUe) &&
                !string.Equals(row.IntituleUe.Trim(), ue.Intitule, StringComparison.OrdinalIgnoreCase))
                errors.Add($"Intitule UE invalide pour l'etudiant {row.NumEtud}: intitule attendu '{ue.Intitule}'.");

            if (string.IsNullOrWhiteSpace(row.NumEtud))
            {
                errors.Add("Une ligne est sans numero etudiant.");
                continue;
            }

            var etudiant = (await etudiantRepo.FindByConditionAsync(e => e.NumEtud == row.NumEtud.Trim()))
                .FirstOrDefault();

            if (etudiant is null)
            {
                errors.Add($"Etudiant inconnu: '{row.NumEtud}'.");
                continue;
            }

            var isInscritDansUe = (await etudiantRepo.FindByConditionAsync(e =>
                e.Id == etudiant.Id &&
                e.ParcoursSuivi != null &&
                e.ParcoursSuivi.UesEnseignees != null &&
                e.ParcoursSuivi.UesEnseignees.Any(u => u.Id == idUe))).Count > 0;

            if (!isInscritDansUe)
                errors.Add($"Etudiant non inscrit a l'UE: '{row.NumEtud}'.");

            if (row.Note is < 0 or > 20)
                errors.Add($"Note hors bornes [0,20] pour '{row.NumEtud}': {row.Note}.");

            upserts.Add((etudiant, row.Note));
        }

        if (errors.Count > 0)
            throw new CsvImportValidationException(errors);

        foreach (var (etudiant, valeur) in upserts)
        {
            var existing = await noteRepo.FindAsync(etudiant.Id, idUe);
            if (existing is null)
            {
                await noteRepo.CreateAsync(new Note
                {
                    EtudiantId = etudiant.Id,
                    UeId = idUe,
                    Valeur = valeur
                });
            }
            else
            {
                existing.Valeur = valeur;
                await noteRepo.UpdateAsync(existing);
            }
        }

        await noteRepo.SaveChangesAsync();
    }
}
