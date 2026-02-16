using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Update;

public class ImportUeNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) => role == Roles.Scolarite;

    public async Task ExecuteAsync(long idUe, List<UeNoteCsvRow> rows)
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

        // On précharge uniquement les étudiants réellement inscrits à l'UE ciblée.
        var etudiantsInscrits = await etudiantRepo.FindByConditionAsync(e =>
            e.ParcoursSuivi != null &&
            e.ParcoursSuivi.UesEnseignees != null &&
            e.ParcoursSuivi.UesEnseignees.Any(u => u.Id == idUe));

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

            if (row.Note is < 0 or > 20)
                errors.Add($"Ligne {line}: note hors bornes [0,20] pour '{numEtud}' ({row.Note}).");

            operations.Add((etudiant.Id, row.Note));
        }

        if (errors.Count > 0)
            throw new CsvImportValidationException(errors.Distinct().ToList());

        // Aucune écriture n'a lieu avant ce point : en cas d'erreur, rien n'est enregistré.
        foreach (var (etudiantId, valeur) in operations)
        {
            var existing = await noteRepo.FindAsync(etudiantId, idUe);
            if (valeur.HasValue)
            {
                if (existing is null)
                {
                    await noteRepo.CreateAsync(new Note
                    {
                        EtudiantId = etudiantId,
                        UeId = idUe,
                        Valeur = valeur.Value
                    });
                }
                else
                {
                    existing.Valeur = valeur.Value;
                    await noteRepo.UpdateAsync(existing);
                }
            }
            else if (existing is not null)
            {
                // Case vide dans le CSV : on supprime la note si elle existait déjà.
                await noteRepo.DeleteAsync(existing);
            }
        }

        await noteRepo.SaveChangesAsync();
    }
}
