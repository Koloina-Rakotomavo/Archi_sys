using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Repositories;

namespace UniversiteEFDataProvider.DataAdaptersFactory;

public class RepositoryFactory(UniversiteDbContext context) : IRepositoryFactory
{
    private readonly UniversiteDbContext dbContext = context;
    private IParcoursRepository? parcoursRepository;
    private IEtudiantRepository? etudiantRepository;
    private IUeRepository? ueRepository;
    private INoteRepository? noteRepository;
    private IUniversiteRoleRepository? universiteRoleRepository;
    private IUniversiteUserRepository? universiteUserRepository;

    public IParcoursRepository ParcoursRepository() =>
        parcoursRepository ??= new ParcoursRepository(dbContext);

    public IEtudiantRepository EtudiantRepository() =>
        etudiantRepository ??= new EtudiantRepository(dbContext);

    public IUeRepository UeRepository() =>
        ueRepository ??= new UeRepository(dbContext);
    
    public INoteRepository NoteRepository() =>
        noteRepository ??= new NoteRepository(dbContext);
    
    public IUniversiteRoleRepository UniversiteRoleRepository() =>
        universiteRoleRepository ??= new UniversiteRoleRepository(dbContext);

    public IUniversiteUserRepository UniversiteUserRepository() =>
        universiteUserRepository ??= new UniversiteUserRepository(dbContext);

    public async Task EnsureDeletedAsync() =>
        await dbContext.Database.EnsureDeletedAsync();

    public async Task EnsureCreatedAsync() =>
        await dbContext.Database.EnsureCreatedAsync();

    public async Task SaveChangesAsync() =>
        await dbContext.SaveChangesAsync();
}
