using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Get;

public class IsInRoleUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) =>
        role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite;

    public async Task<bool> ExecuteAsync(string email, string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        return await repositoryFactory.UniversiteUserRepository().IsInRoleAsync(email, roleName);
    }
}
