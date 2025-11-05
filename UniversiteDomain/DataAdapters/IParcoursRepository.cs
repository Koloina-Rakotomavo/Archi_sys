using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters
{
    public interface IParcoursRepository : IRepository<Parcours>
    {
        // On ajoute plus tard les méthodes spécifiques à Parcours
        // (ex: Task<List<Parcours>> FindByAnneeAsync(int annee); etc.)
    }
}