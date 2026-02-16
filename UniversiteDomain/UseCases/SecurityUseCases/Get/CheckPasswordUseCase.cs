using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.SecurityUseCases.Get;

public class CheckPasswordUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<bool> ExecuteAsync(string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return await repositoryFactory.UniversiteUserRepository().CheckPasswordAsync(email, password);
    }
}
