using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversiteEFDataProvider.Repositories;

public class EtudiantRepository(UniversiteDbContext context) : Repository<Etudiant>(context), IEtudiantRepository
{
    public async Task AffecterParcoursAsync(long idEtudiant, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);

        var etudiant = await Context.Etudiants.FirstOrDefaultAsync(e => e.Id == idEtudiant)
                       ?? throw new InvalidOperationException($"Etudiant {idEtudiant} introuvable.");
        var parcours = await Context.Parcours.FirstOrDefaultAsync(p => p.Id == idParcours)
                       ?? throw new InvalidOperationException($"Parcours {idParcours} introuvable.");

        etudiant.ParcoursSuivi = parcours;
        await Context.SaveChangesAsync();
    }

    public async Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(etudiant);
        ArgumentNullException.ThrowIfNull(parcours);
        await AffecterParcoursAsync(etudiant.Id, parcours.Id);
    }

    public async Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants
            .Include(e => e.ParcoursSuivi!)
            .ThenInclude(p => p.UesEnseignees)
            .Include(e => e.NotesObtenues!)
            .ThenInclude(n => n.Ue)
            .FirstOrDefaultAsync(e => e.Id == idEtudiant);
    }

    public async Task<List<Etudiant>> FindEtudiantsSuivantUeAsync(long ueId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ueId);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);

        var etudiants = await Context.Etudiants
            .Include(e => e.ParcoursSuivi!)
            .ThenInclude(p => p.UesEnseignees)
            .ToListAsync();

        return etudiants
            .Where(e =>
                e.ParcoursSuivi != null &&
                e.ParcoursSuivi.UesEnseignees != null &&
                e.ParcoursSuivi.UesEnseignees.Any(u => u.Id == ueId))
            .ToList();
    }
}
