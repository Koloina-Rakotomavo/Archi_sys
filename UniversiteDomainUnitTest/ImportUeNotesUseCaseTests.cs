using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.UseCases.NoteUseCases.Update;

namespace UniversiteDomainUnitTest;

public class ImportUeNotesUseCaseTests
{
    [Test]
    public void ExecuteAsync_WithInvalidRow_DoesNotPersistAnyChange()
    {
        const long ueId = 10;

        var mockFactory = new Mock<IRepositoryFactory>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        mockUeRepo.Setup(r => r.FindAsync(ueId)).ReturnsAsync(new Ue
        {
            Id = ueId,
            NumeroUe = "UE101",
            Intitule = "Architecture web"
        });

        mockEtudiantRepo
            .Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant>
            {
                new() { Id = 1, NumEtud = "E001" },
                new() { Id = 2, NumEtud = "E002" }
            });

        var rows = new List<UeNoteCsvRow>
        {
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E001", Note = 15m },
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E002", Note = 25m }
        };

        var useCase = new ImportUeNotesUseCase(mockFactory.Object);

        Assert.ThrowsAsync<CsvImportValidationException>(async () => await useCase.ExecuteAsync(ueId, rows));

        mockNoteRepo.Verify(r => r.CreateAsync(It.IsAny<Note>()), Times.Never);
        mockNoteRepo.Verify(r => r.UpdateAsync(It.IsAny<Note>()), Times.Never);
        mockNoteRepo.Verify(r => r.DeleteAsync(It.IsAny<Note>()), Times.Never);
        mockNoteRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WithValidRows_CreatesUpdatesAndDeletesThenSavesOnce()
    {
        const long ueId = 10;

        var mockFactory = new Mock<IRepositoryFactory>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        mockUeRepo.Setup(r => r.FindAsync(ueId)).ReturnsAsync(new Ue
        {
            Id = ueId,
            NumeroUe = "UE101",
            Intitule = "Architecture web"
        });

        mockEtudiantRepo
            .Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant>
            {
                new() { Id = 1, NumEtud = "E001" },
                new() { Id = 2, NumEtud = "E002" },
                new() { Id = 3, NumEtud = "E003" }
            });

        var existingToUpdate = new Note { EtudiantId = 2, UeId = ueId, Valeur = 9m };
        var existingToDelete = new Note { EtudiantId = 3, UeId = ueId, Valeur = 11m };

        mockNoteRepo
            .Setup(r => r.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] keyValues) =>
            {
                var etudiantId = Convert.ToInt64(keyValues[0]);
                var ueKey = Convert.ToInt64(keyValues[1]);
                if (ueKey != ueId)
                    return null;

                return etudiantId switch
                {
                    1 => null,
                    2 => existingToUpdate,
                    3 => existingToDelete,
                    _ => null
                };
            });

        mockNoteRepo.Setup(r => r.CreateAsync(It.IsAny<Note>()))
            .ReturnsAsync((Note n) => n);
        mockNoteRepo.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);
        mockNoteRepo.Setup(r => r.DeleteAsync(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);
        mockNoteRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var rows = new List<UeNoteCsvRow>
        {
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E001", Note = 15m },
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E002", Note = 17.5m },
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E003", Note = null }
        };

        var useCase = new ImportUeNotesUseCase(mockFactory.Object);
        await useCase.ExecuteAsync(ueId, rows);

        mockNoteRepo.Verify(r => r.CreateAsync(It.Is<Note>(n =>
            n.EtudiantId == 1 &&
            n.UeId == ueId &&
            n.Valeur == 15m)), Times.Once);

        mockNoteRepo.Verify(r => r.UpdateAsync(It.Is<Note>(n =>
            n.EtudiantId == 2 &&
            n.UeId == ueId &&
            n.Valeur == 17.5m)), Times.Once);

        mockNoteRepo.Verify(r => r.DeleteAsync(It.Is<Note>(n =>
            n.EtudiantId == 3 &&
            n.UeId == ueId)), Times.Once);

        mockNoteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
