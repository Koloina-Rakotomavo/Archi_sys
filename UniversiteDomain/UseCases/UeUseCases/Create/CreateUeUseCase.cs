using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases;

public class CreateUeUseCase
{
    private readonly IUeRepository ueRepository;

    public CreateUeUseCase(IUeRepository repo)
    {
        ueRepository = repo;
    }

    public async Task<Ue> ExecuteAsync(string numeroUe, string intitule)
    {
        var ue = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);

        var created = await ueRepository.CreateAsync(ue);
        await ueRepository.SaveChangesAsync();

        return created;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);

        // 1. Intitulé ≥ 3 caractères
        if (ue.Intitule.Length < 3)
            throw new InvalidIntituleUeException(ue.Intitule);

        // 2. Numéro unique
        var existants = await ueRepository.FindByConditionAsync(
            u => u.NumeroUe.Equals(ue.NumeroUe)
        );

        if (existants is { Count: > 0 })
            throw new DuplicateNumeroUeException(ue.NumeroUe);
    }
}