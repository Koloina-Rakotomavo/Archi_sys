using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Update;

public class ImportUeNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) => role == Roles.Scolarite;

    public async Task<ImportUeNotesResultDto> ExecuteAsync(long idUe, List<UeNoteCsvRow> rows)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
            throw new CsvImportValidationException(new List<string> { "Le fichier CSV est vide." });

        ArgumentNullException.ThrowIfNull(repositoryFactory);
        var ueRepo = repositoryFactory.UeRepository();
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        var noteRepo = repositoryFactory.NoteRepository();
        ArgumentNullException.ThrowIfNull(ueRepo);
        ArgumentNullException.ThrowIfNull(etudiantRepo);
        ArgumentNullException.ThrowIfNull(noteRepo);

        var ue = await ueRepo.FindAsync(idUe) ?? throw new UeNotFoundException(idUe.ToString());
        var notesExistantes = await noteRepo.FindByUeAsync(idUe);
        var notesByEtudiantId = notesExistantes.ToDictionary(n => n.EtudiantId);

        // On précharge uniquement les étudiants réellement inscrits à l'UE ciblée.
        var etudiantsInscrits = await etudiantRepo.FindEtudiantsSuivantUeAsync(idUe);

        var etudiantsByNum = etudiantsInscrits
            .Where(e => !string.IsNullOrWhiteSpace(e.NumEtud))
            .ToDictionary(e => e.NumEtud.Trim(), StringComparer.OrdinalIgnoreCase);

        var errors = new List<string>();
        var seenNumEtud = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var operations = new List<(long EtudiantId, decimal? Valeur)>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var line = i + 2; // +1 header, +1 index base 1
            var numEtud = row.NumEtud?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(row.NumeroUe) &&
                !string.Equals(row.NumeroUe.Trim(), ue.NumeroUe, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Ligne {line}: numero UE invalide (attendu '{ue.NumeroUe}').");
            }

            if (!string.IsNullOrWhiteSpace(row.IntituleUe) &&
                !string.Equals(row.IntituleUe.Trim(), ue.Intitule, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Ligne {line}: intitule UE invalide (attendu '{ue.Intitule}').");
            }

            if (string.IsNullOrWhiteSpace(numEtud))
            {
                errors.Add($"Ligne {line}: numero etudiant manquant.");
                continue;
            }

            if (!seenNumEtud.Add(numEtud))
            {
                errors.Add($"Ligne {line}: etudiant '{numEtud}' present plusieurs fois dans le CSV.");
                continue;
            }

            if (!etudiantsByNum.TryGetValue(numEtud, out var etudiant))
            {
                errors.Add($"Ligne {line}: etudiant inconnu ou non inscrit a l'UE '{numEtud}'.");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.Nom) &&
                !string.Equals(row.Nom.Trim(), etudiant.Nom, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Ligne {line}: nom etudiant incoherent pour '{numEtud}'.");
            }

            if (!string.IsNullOrWhiteSpace(row.Prenom) &&
                !string.Equals(row.Prenom.Trim(), etudiant.Prenom, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Ligne {line}: prenom etudiant incoherent pour '{numEtud}'.");
            }

            if (row.Note is < 0 or > 20)
                errors.Add($"Ligne {line}: note hors bornes [0,20] pour '{numEtud}' ({row.Note}).");

            operations.Add((etudiant.Id, row.Note));
        }

        var missingStudents = etudiantsByNum.Keys.Where(num => !seenNumEtud.Contains(num)).ToList();
        if (missingStudents.Count > 0)
        {
            foreach (var missing in missingStudents)
                errors.Add($"Etudiant manquant dans le CSV: '{missing}'.");
        }

        if (errors.Count > 0)
            throw new CsvImportValidationException(errors.Distinct().ToList());

        // Aucune écriture n'a lieu avant ce point : en cas d'erreur, rien n'est enregistré.
        var toUpsert = new List<Note>();
        var toDelete = new List<(long EtudiantId, long UeId)>();

        foreach (var (etudiantId, valeur) in operations)
        {
            notesByEtudiantId.TryGetValue(etudiantId, out var existing);
            if (valeur.HasValue)
            {
                toUpsert.Add(new Note
                {
                    EtudiantId = etudiantId,
                    UeId = idUe,
                    Valeur = valeur.Value
                });
            }
            else if (existing is not null)
            {
                // Case vide dans le CSV : on supprime la note si elle existait déjà.
                toDelete.Add((etudiantId, idUe));
            }
        }

        await noteRepo.UpsertManyAsync(toUpsert);
        await noteRepo.DeleteManyAsync(toDelete);
        await noteRepo.SaveChangesAsync();

        return new ImportUeNotesResultDto
        {
            RowsRead = rows.Count,
            UpsertedCount = toUpsert.Count,
            DeletedCount = toDelete.Count
        };
    }
}
