using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteDomainUnitTests;

public class AddUeDansParcoursUnitTests
{
    private Mock<IParcoursRepository> mockParcoursRepo;
    private Mock<IUeRepository> mockUeRepo;
    private Mock<IRepositoryFactory> mockFactory;
    private AddUeDansParcoursUseCase useCase;

    [SetUp]
    public void Setup()
    {
        mockParcoursRepo = new Mock<IParcoursRepository>();
        mockUeRepo = new Mock<IUeRepository>();
        mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(factory => factory.ParcoursRepository()).Returns(mockParcoursRepo.Object);
        mockFactory.Setup(factory => factory.UeRepository()).Returns(mockUeRepo.Object);

        useCase = new AddUeDansParcoursUseCase(mockFactory.Object);
    }

    [Test]
    public async Task AddUeDansParcours_OK()
    {
        var idParcours = 3L;
        var idUe = 10L;
        var ue = new Ue { Id = idUe, NumeroUe = "UE10", Intitule = "Programmation avancee" };
        var parcoursInitial = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1,
            UesEnseignees = new List<Ue>()
        };
        var parcoursFinal = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1,
            UesEnseignees = new List<Ue> { ue }
        };

        mockParcoursRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcoursInitial });
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });
        mockParcoursRepo
            .Setup(repo => repo.AddUeAsync(idParcours, It.Is<long[]>(ids => ids.Length == 1 && ids[0] == idUe)))
            .ReturnsAsync(parcoursFinal);

        var resultat = await useCase.ExecuteAsync(idParcours, idUe);

        Assert.That(resultat.Id, Is.EqualTo(idParcours));
        Assert.That(resultat.UesEnseignees, Is.Not.Null);
        Assert.That(resultat.UesEnseignees!.Count, Is.EqualTo(1));
        Assert.That(resultat.UesEnseignees[0].Id, Is.EqualTo(idUe));
    }

    [Test]
    public void AddUeDansParcours_DoublonDansParcours()
    {
        var idParcours = 3L;
        var idUe = 10L;
        var ue = new Ue { Id = idUe, NumeroUe = "UE10", Intitule = "Programmation avancee" };
        var parcoursInitial = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1,
            UesEnseignees = new List<Ue> { ue }
        };

        mockParcoursRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcoursInitial });
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        Assert.ThrowsAsync<DuplicateUeDansParcoursException>(() => useCase.ExecuteAsync(idParcours, idUe));
    }

    [Test]
    public void AddUeDansParcours_DoublonDansRequete()
    {
        var idParcours = 3L;
        var idUe = 10L;
        var parcoursInitial = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1,
            UesEnseignees = new List<Ue>()
        };

        mockParcoursRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcoursInitial });

        Assert.ThrowsAsync<DuplicateUeDansParcoursException>(() => useCase.ExecuteAsync(idParcours, new[] { idUe, idUe }));
    }
}
