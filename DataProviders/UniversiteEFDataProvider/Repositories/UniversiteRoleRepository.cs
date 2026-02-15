using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entities;

namespace UniversiteEFDataProvider.Repositories;

public class UniversiteRoleRepository(UniversiteDbContext context) : IUniversiteRoleRepository
{
    private readonly UniversiteDbContext dbContext = context;

    public async Task<IUniversiteRole> CreateAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var normalized = roleName.Trim().ToUpperInvariant();

        var existing = await dbContext.UniversiteRoles.FirstOrDefaultAsync(r => r.NormalizedName == normalized);
        if (existing is not null)
            return existing;

        var role = new UniversiteRole
        {
            Name = roleName.Trim(),
            NormalizedName = normalized
        };
        await dbContext.UniversiteRoles.AddAsync(role);
        await dbContext.SaveChangesAsync();
        return role;
    }

    public async Task<IUniversiteRole?> FindByNameAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var normalized = roleName.Trim().ToUpperInvariant();
        return await dbContext.UniversiteRoles.FirstOrDefaultAsync(r => r.NormalizedName == normalized);
    }

    public async Task<bool> ExistsAsync(string roleName) =>
        await FindByNameAsync(roleName) is not null;
}
