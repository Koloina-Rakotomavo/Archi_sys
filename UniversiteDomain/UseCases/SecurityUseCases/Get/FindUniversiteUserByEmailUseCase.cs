using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Get;

public class FindUniversiteUserByEmailUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) =>
        role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite or Roles.Etudiant;

    public async Task<IUniversiteUser?> ExecuteAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return await repositoryFactory.UniversiteUserRepository().FindByEmailAsync(email);
    }
}
