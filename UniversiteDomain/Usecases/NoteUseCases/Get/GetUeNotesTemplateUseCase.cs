using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Get;

public class GetUeNotesTemplateUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<List<UeNoteCsvRow>> ExecuteAsync(long idUe)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);

        var ueRepo = repositoryFactory.UeRepository();
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        var noteRepo = repositoryFactory.NoteRepository();

        var ue = await ueRepo.FindAsync(idUe) ?? throw new UeNotFoundException(idUe.ToString());

        var etudiants = await etudiantRepo.FindByConditionAsync(e =>
            e.ParcoursSuivi != null &&
            e.ParcoursSuivi.UesEnseignees != null &&
            e.ParcoursSuivi.UesEnseignees.Any(u => u.Id == idUe));

        var rows = new List<UeNoteCsvRow>();
        foreach (var etudiant in etudiants.OrderBy(e => e.NumEtud))
        {
            var existing = await noteRepo.FindAsync(etudiant.Id, idUe);
            rows.Add(new UeNoteCsvRow
            {
                NumeroUe = ue.NumeroUe,
                IntituleUe = ue.Intitule,
                NumEtud = etudiant.NumEtud,
                Nom = etudiant.Nom,
                Prenom = etudiant.Prenom,
                Note = existing?.Valeur
            });
        }

        return rows;
    }
}
