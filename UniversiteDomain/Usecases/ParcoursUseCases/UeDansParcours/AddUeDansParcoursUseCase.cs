using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

public class AddUeDansParcoursUseCase
{
    private readonly IRepositoryFactory repositoryFactory;

    public AddUeDansParcoursUseCase(IRepositoryFactory factory)
    {
        repositoryFactory = factory;
    }

    // ------------------------------------------------------------
    // 1. Ajout d'une UE via objets
    // ------------------------------------------------------------
    public async Task<Parcours> ExecuteAsync(Parcours parcours, Ue ue)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(ue);

        return await ExecuteAsync(parcours.Id, ue.Id);
    }

    // ------------------------------------------------------------
    // 2. Ajout d'une UE via ids
    // ------------------------------------------------------------
    public async Task<Parcours> ExecuteAsync(long idParcours, long[] idUe)
    {
        await CheckBusinessRules(idParcours, idUe);

        return await repositoryFactory.ParcoursRepository()
            .AddUeAsync(idParcours, idUe);
    }

    // ------------------------------------------------------------
    // 3. Ajout d'une LISTE d'UE via objets
    // ------------------------------------------------------------
    public async Task<Parcours> ExecuteAsync(Parcours parcours, List<Ue> ues)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(ues);

        long[] ids = ues.Select(u => u.Id).ToArray();
        return await ExecuteAsync(parcours.Id, ids);
    }

    // ------------------------------------------------------------
    // 4. Ajout d'une LISTE d'UE via ids
    // ------------------------------------------------------------
    public async Task<Parcours> ExecuteAsync(long idParcours, long[] idUes)
    {
        ArgumentNullException.ThrowIfNull(idUes);

        // Vérification de TOUTES les règles métier AVANT modification
        foreach (var id in idUes)
            await CheckBusinessRules(idParcours, id);

        return await repositoryFactory.ParcoursRepository()
            .AddUeAsync(idParcours, idUes);
    }

    // ------------------------------------------------------------
    // 5. Règles métier
    // ------------------------------------------------------------
    private async Task CheckBusinessRules(long idParcours, long idUe)
    {
        if (idParcours <= 0)
            throw new ArgumentOutOfRangeException(nameof(idParcours));

        if (idUe <= 0)
            throw new ArgumentOutOfRangeException(nameof(idUe));

        // Vérification des dépendances
        if (repositoryFactory == null)
            throw new ArgumentNullException(nameof(repositoryFactory));

        var ueRepo = repositoryFactory.UeRepository();
        var parcoursRepo = repositoryFactory.ParcoursRepository();

        ArgumentNullException.ThrowIfNull(ueRepo);
        ArgumentNullException.ThrowIfNull(parcoursRepo);

        // 1. L’UE existe ?
        var ueList = await ueRepo.FindByConditionAsync(u => u.Id == idUe);
        if (ueList == null || ueList.Count == 0)
            throw new UeNotFoundException(idUe.ToString());

        // 2. Le parcours existe ?
        var parcoursList = await parcoursRepo.FindByConditionAsync(p => p.Id == idParcours);
        if (parcoursList == null || parcoursList.Count == 0)
            throw new ParcoursNotFoundException(idParcours.ToString());

        var parcours = parcoursList[0];

        // 3. L’UE n’est pas déjà présente dans le parcours
        if (parcours.UesEnseignees != null)
        {
            bool existe = parcours.UesEnseignees.Any(u => u.Id == idUe);
            if (existe)
                throw new DuplicateUeDansParcoursException(idUe, idParcours);
        }
    }
}
