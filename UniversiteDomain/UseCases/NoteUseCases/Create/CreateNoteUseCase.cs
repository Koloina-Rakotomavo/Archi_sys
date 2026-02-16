using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class CreateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) =>
        role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite;

    public async Task<Note> ExecuteAsync(long idEtudiant, long idUe, decimal valeur)
    {
        var note = new Note
        {
            EtudiantId = idEtudiant,
            UeId = idUe,
            Valeur = valeur
        };
        return await ExecuteAsync(note);
    }

    public async Task<Note> ExecuteAsync(Note note)
    {
        await CheckBusinessRules(note);
        var noteRepository = repositoryFactory.NoteRepository();
        var created = await noteRepository.CreateAsync(note);
        await noteRepository.SaveChangesAsync();
        return created;
    }

    private async Task CheckBusinessRules(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.EtudiantId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.UeId);

        if (!note.Valeur.HasValue)
            throw new InvalidNoteException("La note est obligatoire pour cette operation.");

        if (note.Valeur is < 0 or > 20)
            throw new InvalidNoteException($"La note {note.Valeur} est hors bornes [0,20].");

        var etudiantRepository = repositoryFactory.EtudiantRepository();
        var ueRepository = repositoryFactory.UeRepository();
        var noteRepository = repositoryFactory.NoteRepository();

        var etudiant = await etudiantRepository.FindEtudiantCompletAsync(note.EtudiantId)
                       ?? throw new EtudiantNotFoundException(note.EtudiantId.ToString());
        _ = await ueRepository.FindAsync(note.UeId)
            ?? throw new UeNotFoundException(note.UeId.ToString());

        var dejaPresente = await noteRepository.FindAsync(note.EtudiantId, note.UeId);
        if (dejaPresente is not null)
            throw new DuplicateNoteException($"Une note existe deja pour l'etudiant {note.EtudiantId} dans l'UE {note.UeId}.");

        var estInscritDansUe =
            etudiant.ParcoursSuivi?.UesEnseignees?.Any(u => u.Id == note.UeId) ?? false;

        if (!estInscritDansUe)
        {
            throw new InvalidNoteEtudiantException(
                $"L'etudiant {note.EtudiantId} n'est pas inscrit dans l'UE {note.UeId}.");
        }
    }
}
