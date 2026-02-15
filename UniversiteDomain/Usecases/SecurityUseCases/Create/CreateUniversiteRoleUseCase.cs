using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Create;

public class CreateUniversiteRoleUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) =>
        role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite;

    public async Task<IUniversiteRole> ExecuteAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var repo = repositoryFactory.UniversiteRoleRepository();

        if (await repo.ExistsAsync(roleName))
        {
            var existing = await repo.FindByNameAsync(roleName);
            return existing!;
        }

        return await repo.CreateAsync(roleName);
    }
}
