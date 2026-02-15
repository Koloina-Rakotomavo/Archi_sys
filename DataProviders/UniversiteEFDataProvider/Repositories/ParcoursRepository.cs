using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository(UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    public Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(etudiant);
        return AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        var parcours = await LoadParcoursWithEtudiantsAsync(idParcours);
        var etudiant = await Context.Etudiants.FirstOrDefaultAsync(e => e.Id == idEtudiant)
                       ?? throw new InvalidOperationException($"Etudiant {idEtudiant} introuvable.");

        parcours.Inscrits ??= new List<Etudiant>();
        if (parcours.Inscrits.All(e => e.Id != etudiant.Id))
            parcours.Inscrits.Add(etudiant);

        etudiant.ParcoursSuivi = parcours;
        await Context.SaveChangesAsync();
        return parcours;
    }

    public Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(etudiants);
        var ids = etudiants.Select(e => e.Id).Distinct().ToArray();
        return AddEtudiantAsync(parcours.Id, ids);
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        ArgumentNullException.ThrowIfNull(idEtudiants);
        var ids = idEtudiants.Distinct().ToArray();
        var parcours = await LoadParcoursWithEtudiantsAsync(idParcours);

        var etudiants = await Context.Etudiants
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        var missingId = ids.FirstOrDefault(id => etudiants.All(e => e.Id != id));
        if (missingId != 0)
            throw new InvalidOperationException($"Etudiant {missingId} introuvable.");

        parcours.Inscrits ??= new List<Etudiant>();
        foreach (var etudiant in etudiants)
        {
            if (parcours.Inscrits.All(e => e.Id != etudiant.Id))
                parcours.Inscrits.Add(etudiant);

            etudiant.ParcoursSuivi = parcours;
        }

        await Context.SaveChangesAsync();
        return parcours;
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUes)
    {
        ArgumentNullException.ThrowIfNull(idUes);
        var ids = idUes.Distinct().ToArray();
        var parcours = await LoadParcoursWithUesAsync(idParcours);

        var ues = await Context.Ues
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();

        var missingId = ids.FirstOrDefault(id => ues.All(u => u.Id != id));
        if (missingId != 0)
            throw new InvalidOperationException($"UE {missingId} introuvable.");

        parcours.UesEnseignees ??= new List<Ue>();
        foreach (var ue in ues.Where(ue => parcours.UesEnseignees.All(existing => existing.Id != ue.Id)))
            parcours.UesEnseignees.Add(ue);

        await Context.SaveChangesAsync();
        return parcours;
    }

    public Task<Parcours> AddUeAsync(Parcours parcours, Ue ue)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(ue);
        return AddUeAsync(parcours.Id, ue.Id);
    }

    public Task<Parcours> AddUeAsync(long idParcours, long idUe) =>
        AddUeAsync(idParcours, new[] { idUe });

    private async Task<Parcours> LoadParcoursWithEtudiantsAsync(long idParcours) =>
        await Context.Parcours
            .Include(p => p.Inscrits)
            .FirstOrDefaultAsync(p => p.Id == idParcours)
        ?? throw new InvalidOperationException($"Parcours {idParcours} introuvable.");

    private async Task<Parcours> LoadParcoursWithUesAsync(long idParcours) =>
        await Context.Parcours
            .Include(p => p.UesEnseignees)
            .FirstOrDefaultAsync(p => p.Id == idParcours)
        ?? throw new InvalidOperationException($"Parcours {idParcours} introuvable.");
}
