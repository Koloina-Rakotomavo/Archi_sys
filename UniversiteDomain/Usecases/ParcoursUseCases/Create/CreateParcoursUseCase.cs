using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory factory)
{
    public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours
        {
            NomParcours = nomParcours,
            AnneeFormation = anneeFormation
        };

        return await ExecuteAsync(parcours);
    }

    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);

        var repo = factory.ParcoursRepository();   // CORRECTION IMPORTANTE

        var created = await repo.CreateAsync(parcours);
        await factory.SaveChangesAsync();         // on utilise la factory pour commit

        return created;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);

        var repo = factory.ParcoursRepository();   // CORRECTION IMPORTANTE

        // 1. nom ≥ 3 caractères


        // 2. vérifier doublon
        var existants = await repo.FindByConditionAsync(
            p => p.NomParcours.Equals(parcours.NomParcours)
        );

        if (existants is { Count: > 0 })
        {
            throw new DuplicateNomParcoursException(parcours.NomParcours);
        }

        // 3. année de formation valide
        if (parcours.AnneeFormation < 1 || parcours.AnneeFormation > 5)
        {
            throw new InvalidAnneeParcoursException(parcours.AnneeFormation);
        }
    }
}