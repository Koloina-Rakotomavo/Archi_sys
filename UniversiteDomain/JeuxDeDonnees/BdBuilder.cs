using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.JeuxDeDonnees;

public abstract class BdBuilder(IRepositoryFactory repositoryFactory)
{
    protected readonly IRepositoryFactory RepositoryFactory = repositoryFactory;

    public async Task BuildAsync()
    {
        await RepositoryFactory.EnsureDeletedAsync();
        await RepositoryFactory.EnsureCreatedAsync();

        await BuildMetierAsync();
        await BuildSecuriteAsync();

        await RepositoryFactory.SaveChangesAsync();
    }

    protected abstract Task BuildMetierAsync();
    protected abstract Task BuildSecuriteAsync();
}
