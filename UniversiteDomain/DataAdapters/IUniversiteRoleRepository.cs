using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteRoleRepository
{
    Task<IUniversiteRole> CreateAsync(string roleName);
    Task<IUniversiteRole?> FindByNameAsync(string roleName);
    Task<bool> ExistsAsync(string roleName);
}
