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
            .Setup(r => r.FindEtudiantsSuivantUeAsync(ueId))
            .ReturnsAsync(new List<Etudiant>
            {
                new() { Id = 1, NumEtud = "E001" },
                new() { Id = 2, NumEtud = "E002" }
            });

        mockNoteRepo
            .Setup(r => r.FindByUeAsync(ueId))
            .ReturnsAsync(new List<Note>());

        var rows = new List<UeNoteCsvRow>
        {
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E001", Note = 15m },
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E002", Note = 25m }
        };

        var useCase = new ImportUeNotesUseCase(mockFactory.Object);

        Assert.ThrowsAsync<CsvImportValidationException>(async () => await useCase.ExecuteAsync(ueId, rows));

        mockNoteRepo.Verify(r => r.UpsertManyAsync(It.IsAny<List<Note>>()), Times.Never);
        mockNoteRepo.Verify(r => r.DeleteManyAsync(It.IsAny<List<(long, long)>>()), Times.Never);
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
            .Setup(r => r.FindEtudiantsSuivantUeAsync(ueId))
            .ReturnsAsync(new List<Etudiant>
            {
                new() { Id = 1, NumEtud = "E001" },
                new() { Id = 2, NumEtud = "E002" },
                new() { Id = 3, NumEtud = "E003" }
            });

        mockNoteRepo
            .Setup(r => r.FindByUeAsync(ueId))
            .ReturnsAsync(new List<Note>
            {
                new() { EtudiantId = 2, UeId = ueId, Valeur = 9m },
                new() { EtudiantId = 3, UeId = ueId, Valeur = 11m }
            });

        mockNoteRepo.Setup(r => r.UpsertManyAsync(It.IsAny<List<Note>>()))
            .Returns(Task.CompletedTask);
        mockNoteRepo.Setup(r => r.DeleteManyAsync(It.IsAny<List<(long, long)>>()))
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
        var result = await useCase.ExecuteAsync(ueId, rows);

        Assert.That(result.RowsRead, Is.EqualTo(3));
        Assert.That(result.UpsertedCount, Is.EqualTo(2));
        Assert.That(result.DeletedCount, Is.EqualTo(1));

        mockNoteRepo.Verify(r => r.UpsertManyAsync(It.Is<List<Note>>(notes =>
            notes.Count == 2 &&
            notes.Any(n => n.EtudiantId == 1 && n.UeId == ueId && n.Valeur == 15m) &&
            notes.Any(n => n.EtudiantId == 2 && n.UeId == ueId && n.Valeur == 17.5m))), Times.Once);

        mockNoteRepo.Verify(r => r.DeleteManyAsync(It.Is<List<(long, long)>>(keys =>
            keys.Count == 1 &&
            keys[0].Item1 == 3 &&
            keys[0].Item2 == ueId)), Times.Once);

        mockNoteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void ExecuteAsync_WithMissingStudentRow_ShouldFailAndWriteNothing()
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
            .Setup(r => r.FindEtudiantsSuivantUeAsync(ueId))
            .ReturnsAsync(new List<Etudiant>
            {
                new() { Id = 1, NumEtud = "E001", Nom = "Durand", Prenom = "Alice" },
                new() { Id = 2, NumEtud = "E002", Nom = "Martin", Prenom = "Leo" }
            });

        mockNoteRepo.Setup(r => r.FindByUeAsync(ueId)).ReturnsAsync(new List<Note>());

        var rows = new List<UeNoteCsvRow>
        {
            new() { NumeroUe = "UE101", IntituleUe = "Architecture web", NumEtud = "E001", Nom = "Durand", Prenom = "Alice", Note = 14m }
        };

        var useCase = new ImportUeNotesUseCase(mockFactory.Object);
        Assert.ThrowsAsync<CsvImportValidationException>(async () => await useCase.ExecuteAsync(ueId, rows));

        mockNoteRepo.Verify(r => r.UpsertManyAsync(It.IsAny<List<Note>>()), Times.Never);
        mockNoteRepo.Verify(r => r.DeleteManyAsync(It.IsAny<List<(long, long)>>()), Times.Never);
        mockNoteRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
