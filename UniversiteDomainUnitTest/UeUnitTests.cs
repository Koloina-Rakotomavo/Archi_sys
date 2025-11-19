using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;
using UniversiteDomain.UseCases.UeUseCases;

namespace UniversiteDomainUnitTests;

public class UeUnitTests
{
    private Mock<IUeRepository> mockUeRepo;
    private CreateUeUseCase useCase;

    [SetUp]
    public void Setup()
    {
        mockUeRepo = new Mock<IUeRepository>();
        useCase = new CreateUeUseCase(mockUeRepo.Object);
    }

    [Test]
    public async Task CreateUe_OK()
    {
        // Arrange
        var ueInitiale = new Ue { NumeroUe = "UE101", Intitule = "Programmation" };
        var ueCreee = new Ue { Id = 1, NumeroUe = "UE101", Intitule = "Programmation" };

        mockUeRepo.Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>()); // pas de doublon

        mockUeRepo.Setup(r => r.CreateAsync(ueInitiale))
            .ReturnsAsync(ueCreee);

        // Act
        var resultat = await useCase.ExecuteAsync(ueInitiale);

        // Assert
        Assert.That(resultat.Id, Is.EqualTo(1));
        Assert.That(resultat.NumeroUe, Is.EqualTo("UE101"));
        Assert.That(resultat.Intitule, Is.EqualTo("Programmation"));
    }

    [Test]
    public void CreateUe_IntituleInvalide()
    {
        // Arrange
        var ue = new Ue { NumeroUe = "UE102", Intitule = "AI" }; // trop court

        mockUeRepo.Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // Act + Assert
        Assert.ThrowsAsync<InvalidIntituleUeException>(() => useCase.ExecuteAsync(ue));
    }

    [Test]
    public void CreateUe_DuplicateNumero()
    {
        // Arrange
        var ue = new Ue { NumeroUe = "UE103", Intitule = "Mathématiques" };

        mockUeRepo.Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { new Ue() }); // simulate duplicate

        // Act + Assert
        Assert.ThrowsAsync<DuplicateNumeroUeException>(() => useCase.ExecuteAsync(ue));
    }
}
