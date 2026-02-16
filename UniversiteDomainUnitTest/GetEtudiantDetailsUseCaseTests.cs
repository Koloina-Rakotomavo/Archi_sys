using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Get;

namespace UniversiteDomainUnitTest;

public class GetEtudiantDetailsUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ReturnsEtudiantComplet()
    {
        var mockFactory = new Mock<IRepositoryFactory>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);

        var expected = new Etudiant
        {
            Id = 5,
            NumEtud = "E005",
            Nom = "Durand",
            Prenom = "Lea"
        };
        mockEtudiantRepo.Setup(r => r.FindEtudiantCompletAsync(5)).ReturnsAsync(expected);

        var useCase = new GetEtudiantDetailsUseCase(mockFactory.Object);
        var result = await useCase.ExecuteAsync(5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(5));
        mockEtudiantRepo.Verify(r => r.FindEtudiantCompletAsync(5), Times.Once);
    }

    [Test]
    public void IsAuthorized_ForScolarite_ReturnsTrue()
    {
        var useCase = new GetEtudiantDetailsUseCase(new Mock<IRepositoryFactory>().Object);
        var authorized = useCase.IsAuthorized(Roles.Scolarite, null, 10);
        Assert.That(authorized, Is.True);
    }

    [Test]
    public void IsAuthorized_ForEtudiantOwnRecord_ReturnsTrue()
    {
        var useCase = new GetEtudiantDetailsUseCase(new Mock<IRepositoryFactory>().Object);
        var user = new FakeUser { EtudiantLieId = 10 };
        var authorized = useCase.IsAuthorized(Roles.Etudiant, user, 10);
        Assert.That(authorized, Is.True);
    }

    [Test]
    public void IsAuthorized_ForEtudiantOtherRecord_ReturnsFalse()
    {
        var useCase = new GetEtudiantDetailsUseCase(new Mock<IRepositoryFactory>().Object);
        var user = new FakeUser { EtudiantLieId = 11 };
        var authorized = useCase.IsAuthorized(Roles.Etudiant, user, 10);
        Assert.That(authorized, Is.False);
    }

    private class FakeUser : IUniversiteUser
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Email { get; init; } = "test@etud.u-picardie.fr";
        public long? EtudiantLieId { get; init; }
        public Etudiant? EtudiantLie { get; init; }
    }
}
