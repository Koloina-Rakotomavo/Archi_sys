using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

public class AddUeDansParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Parcours> ExecuteAsync(Parcours parcours, Ue ue)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(ue);
        return await ExecuteAsync(parcours.Id, ue.Id);
    }

    public Task<Parcours> ExecuteAsync(long idParcours, long idUe) =>
        ExecuteAsync(idParcours, new[] { idUe });

    public async Task<Parcours> ExecuteAsync(Parcours parcours, List<Ue> ues)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(ues);
        var ids = ues.Select(u => u.Id).ToArray();
        return await ExecuteAsync(parcours.Id, ids);
    }

    public async Task<Parcours> ExecuteAsync(long idParcours, long[] idUes)
    {
        await CheckBusinessRules(idParcours, idUes);
        return await repositoryFactory.ParcoursRepository()
            .AddUeAsync(idParcours, idUes);
    }

    private async Task CheckBusinessRules(long idParcours, long[] idUes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idParcours);
        ArgumentNullException.ThrowIfNull(idUes);
        if (idUes.Length == 0)
            throw new ArgumentException("La liste des UEs ne peut pas être vide.", nameof(idUes));
        if (idUes.Any(id => id <= 0))
            throw new ArgumentOutOfRangeException(nameof(idUes), "Tous les identifiants d'UE doivent être strictement positifs.");

        ArgumentNullException.ThrowIfNull(repositoryFactory);
        var ueRepo = repositoryFactory.UeRepository();
        var parcoursRepo = repositoryFactory.ParcoursRepository();
        ArgumentNullException.ThrowIfNull(ueRepo);
        ArgumentNullException.ThrowIfNull(parcoursRepo);

        var parcoursList = await parcoursRepo.FindByConditionAsync(p => p.Id.Equals(idParcours));
        if (parcoursList == null || parcoursList.Count == 0)
            throw new ParcoursNotFoundException(idParcours.ToString());

        var parcours = parcoursList[0];

        var duplicateRequested = idUes
            .GroupBy(id => id)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateRequested != null)
            throw new DuplicateUeDansParcoursException(duplicateRequested.Key, idParcours);

        foreach (var idUe in idUes)
        {
            var ueList = await ueRepo.FindByConditionAsync(u => u.Id.Equals(idUe));
            if (ueList == null || ueList.Count == 0)
                throw new UeNotFoundException(idUe.ToString());

            var existeDansParcours = parcours.UesEnseignees?.Any(u => u.Id.Equals(idUe)) ?? false;
            if (existeDansParcours)
                throw new DuplicateUeDansParcoursException(idUe, idParcours);
        }
    }
}
