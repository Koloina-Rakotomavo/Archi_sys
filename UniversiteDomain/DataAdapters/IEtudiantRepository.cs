using UniversiteDomain.Entities;
namespace UniversiteDomain.DataAdapters;

public interface IEtudiantRepository : IRepository<Etudiant>
{
    Task AffecterParcoursAsync(long idEtudiant, long idParcours);
    Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours);
    Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant);
    Task<List<Etudiant>> FindEtudiantsSuivantUeAsync(long ueId);
}
