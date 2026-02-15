using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entities;

namespace UniversiteEFDataProvider.Repositories;

public class UniversiteUserRepository(UniversiteDbContext context) : IUniversiteUserRepository
{
    private readonly UniversiteDbContext dbContext = context;
    private readonly PasswordHasher<UniversiteUser> passwordHasher = new();

    public async Task<IUniversiteUser> CreateAsync(string email, string password, long? etudiantLieId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var existing = await dbContext.UniversiteUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (existing is not null)
            return existing;

        var user = new UniversiteUser
        {
            UserName = email.Trim(),
            NormalizedUserName = normalizedEmail,
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            EtudiantLieId = etudiantLieId
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        await dbContext.UniversiteUsers.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<IUniversiteUser?> FindByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.Trim().ToUpperInvariant();
        return await dbContext.UniversiteUsers
            .Include(u => u.EtudiantLie)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
    }

    public async Task<bool> CheckPasswordAsync(string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await dbContext.UniversiteUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return false;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public async Task AddToRoleAsync(string email, string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var normalizedRole = roleName.Trim().ToUpperInvariant();

        var user = await dbContext.UniversiteUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail)
                   ?? throw new InvalidOperationException($"Utilisateur introuvable: {email}");
        var role = await dbContext.UniversiteRoles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRole)
                   ?? throw new InvalidOperationException($"Role introuvable: {roleName}");

        var userRoleSet = dbContext.Set<IdentityUserRole<string>>();
        var alreadyInRole = await userRoleSet.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (alreadyInRole)
            return;

        await userRoleSet.AddAsync(new IdentityUserRole<string>
        {
            UserId = user.Id,
            RoleId = role.Id
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsInRoleAsync(string email, string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var normalizedRole = roleName.Trim().ToUpperInvariant();

        var user = await dbContext.UniversiteUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        var role = await dbContext.UniversiteRoles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRole);
        if (user is null || role is null)
            return false;

        return await dbContext.Set<IdentityUserRole<string>>()
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
    }
}
