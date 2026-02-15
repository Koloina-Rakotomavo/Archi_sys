using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Create;

public class CreateUniversiteUserUseCase(IRepositoryFactory repositoryFactory)
{
    public bool IsAuthorized(string role) =>
        role is Roles.Administrateur or Roles.Responsable or Roles.Scolarite;

    public async Task<IUniversiteUser> ExecuteAsync(
        string email,
        string password,
        string roleName,
        long? etudiantLieId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        var roleRepo = repositoryFactory.UniversiteRoleRepository();
        if (!await roleRepo.ExistsAsync(roleName))
            await roleRepo.CreateAsync(roleName);

        var userRepo = repositoryFactory.UniversiteUserRepository();
        var existing = await userRepo.FindByEmailAsync(email);
        if (existing is not null)
        {
            await userRepo.AddToRoleAsync(email, roleName);
            return existing;
        }

        var created = await userRepo.CreateAsync(email, password, etudiantLieId);
        await userRepo.AddToRoleAsync(email, roleName);
        return created;
    }
}
