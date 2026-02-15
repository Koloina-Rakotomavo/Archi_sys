namespace UniversiteDomain.Entities;

public interface IUniversiteUser
{
    string Id { get; }
    string Email { get; }
    long? EtudiantLieId { get; }
    Etudiant? EtudiantLie { get; }
}
