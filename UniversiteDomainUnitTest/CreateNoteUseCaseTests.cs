using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.UseCases.NoteUseCases.Create;

namespace UniversiteDomainUnitTest;

public class CreateNoteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_WithValidData_CreatesNote()
    {
        var mockFactory = new Mock<IRepositoryFactory>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        var etudiantId = 2L;
        var ueId = 10L;
        var note = new Note { EtudiantId = etudiantId, UeId = ueId, Valeur = 14.5m };

        mockEtudiantRepo.Setup(r => r.FindEtudiantCompletAsync(etudiantId))
            .ReturnsAsync(new Etudiant
            {
                Id = etudiantId,
                NumEtud = "E002",
                ParcoursSuivi = new Parcours
                {
                    Id = 1,
                    UesEnseignees = new List<Ue> { new() { Id = ueId } }
                }
            });
        mockUeRepo.Setup(r => r.FindAsync(ueId))
            .ReturnsAsync(new Ue { Id = ueId, NumeroUe = "UE101", Intitule = "Architecture web" });
        mockNoteRepo.Setup(r => r.FindAsync(etudiantId, ueId))
            .ReturnsAsync((Note?)null);
        mockNoteRepo.Setup(r => r.CreateAsync(It.IsAny<Note>()))
            .ReturnsAsync((Note n) => n);

        var useCase = new CreateNoteUseCase(mockFactory.Object);
        var result = await useCase.ExecuteAsync(note);

        Assert.That(result.EtudiantId, Is.EqualTo(etudiantId));
        Assert.That(result.UeId, Is.EqualTo(ueId));
        Assert.That(result.Valeur, Is.EqualTo(14.5m));
        mockNoteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void ExecuteAsync_WhenNoteAlreadyExists_ThrowsDuplicate()
    {
        var mockFactory = new Mock<IRepositoryFactory>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        var etudiantId = 2L;
        var ueId = 10L;

        mockEtudiantRepo.Setup(r => r.FindEtudiantCompletAsync(etudiantId))
            .ReturnsAsync(new Etudiant
            {
                Id = etudiantId,
                ParcoursSuivi = new Parcours
                {
                    Id = 1,
                    UesEnseignees = new List<Ue> { new() { Id = ueId } }
                }
            });
        mockUeRepo.Setup(r => r.FindAsync(ueId))
            .ReturnsAsync(new Ue { Id = ueId });
        mockNoteRepo.Setup(r => r.FindAsync(etudiantId, ueId))
            .ReturnsAsync(new Note { EtudiantId = etudiantId, UeId = ueId, Valeur = 11m });

        var useCase = new CreateNoteUseCase(mockFactory.Object);

        Assert.ThrowsAsync<DuplicateNoteException>(() => useCase.ExecuteAsync(etudiantId, ueId, 15m));
        mockNoteRepo.Verify(r => r.CreateAsync(It.IsAny<Note>()), Times.Never);
    }

    [Test]
    public void ExecuteAsync_WhenStudentNotEnrolledInUe_Throws()
    {
        var mockFactory = new Mock<IRepositoryFactory>();
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        var etudiantId = 2L;
        var ueId = 10L;

        mockEtudiantRepo.Setup(r => r.FindEtudiantCompletAsync(etudiantId))
            .ReturnsAsync(new Etudiant
            {
                Id = etudiantId,
                ParcoursSuivi = new Parcours
                {
                    Id = 1,
                    UesEnseignees = new List<Ue>()
                }
            });
        mockUeRepo.Setup(r => r.FindAsync(ueId))
            .ReturnsAsync(new Ue { Id = ueId });
        mockNoteRepo.Setup(r => r.FindAsync(etudiantId, ueId))
            .ReturnsAsync((Note?)null);

        var useCase = new CreateNoteUseCase(mockFactory.Object);

        Assert.ThrowsAsync<InvalidNoteEtudiantException>(() => useCase.ExecuteAsync(etudiantId, ueId, 15m));
        mockNoteRepo.Verify(r => r.CreateAsync(It.IsAny<Note>()), Times.Never);
    }
}
