using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory factory)
{
    public bool IsAuthorized(string role) =>
        role == Roles.Administrateur || role == Roles.Responsable || role == Roles.Scolarite;

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

        var repo = factory.ParcoursRepository();

        var created = await repo.CreateAsync(parcours);
        await factory.SaveChangesAsync();

        return created;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);

        var repo = factory.ParcoursRepository();
        var nomParcours = parcours.NomParcours.Trim();

        // 1. nom ≥ 3 caractères
        if (nomParcours.Length < 3)
        {
            throw new InvalidNomParcoursException(nomParcours);
        }

        // 2. vérifier doublon sur (NomParcours, AnneeFormation)
        var existants = await repo.FindByConditionAsync(
            p => p.NomParcours.ToLower().Equals(nomParcours.ToLower()) &&
                 p.AnneeFormation.Equals(parcours.AnneeFormation)
        );

        if (existants is { Count: > 0 })
        {
            throw new DuplicateNomParcoursException($"{nomParcours} (annee {parcours.AnneeFormation})");
        }

        // 3. année de formation valide
        if (parcours.AnneeFormation < 1 || parcours.AnneeFormation > 5)
        {
            throw new InvalidAnneeParcoursException(parcours.AnneeFormation);
        }
    }
}
