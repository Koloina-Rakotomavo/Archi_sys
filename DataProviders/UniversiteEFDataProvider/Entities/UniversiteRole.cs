using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entities;

namespace UniversiteEFDataProvider.Entities;

public class UniversiteRole : IdentityRole, IUniversiteRole
{
    string IUniversiteRole.Id => Id;
    string IUniversiteRole.Name => Name ?? string.Empty;
}
