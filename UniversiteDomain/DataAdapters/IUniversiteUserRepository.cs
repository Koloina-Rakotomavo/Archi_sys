using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteUserRepository
{
    Task<IUniversiteUser> CreateAsync(string email, string password, long? etudiantLieId = null);
    Task<IUniversiteUser?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(string email, string password);
    Task AddToRoleAsync(string email, string roleName);
    Task<bool> IsInRoleAsync(string email, string roleName);
}
