using Microsoft.AspNetCore.Identity;
using UniversiteDomain.Entities;

namespace UniversiteEFDataProvider.Entities;

public class UniversiteUser : IdentityUser, IUniversiteUser
{
    public long? EtudiantLieId { get; set; }
    public Etudiant? EtudiantLie { get; set; }

    string IUniversiteUser.Id => Id;
    string IUniversiteUser.Email => Email ?? string.Empty;
}
