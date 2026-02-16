using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Get;

public class GetEtudiantDetailsUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Etudiant?> ExecuteAsync(long idEtudiant)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        return await repositoryFactory.EtudiantRepository().FindEtudiantCompletAsync(idEtudiant);
    }

    public bool IsAuthorized(string role, IUniversiteUser? connectedUser, long requestedEtudiantId)
    {
        if (role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite)
            return true;

        if (role == Roles.Etudiant && connectedUser is not null)
            return connectedUser.EtudiantLieId == requestedEtudiantId;

        return false;
    }
}
