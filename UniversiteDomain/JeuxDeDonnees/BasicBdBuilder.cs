using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.ParcoursUseCases.Create;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;
using UniversiteDomain.UseCases.SecurityUseCases.Create;
using UniversiteDomain.UseCases.UeUseCases;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.JeuxDeDonnees;

public class BasicBdBuilder(IRepositoryFactory repositoryFactory) : BdBuilder(repositoryFactory)
{
    private readonly List<Etudiant> etudiants = new();
    private readonly List<Parcours> parcours = new();
    private readonly List<Ue> ues = new();

    protected override async Task BuildMetierAsync()
    {
        var createParcours = new CreateParcoursUseCase(RepositoryFactory);
        var createEtudiant = new CreateEtudiantUseCase(RepositoryFactory.EtudiantRepository());
        var createUe = new CreateUeUseCase(RepositoryFactory.UeRepository());
        var addEtudiantDansParcours = new AddEtudiantDansParcoursUseCase(RepositoryFactory);
        var addUeDansParcours = new AddUeDansParcoursUseCase(RepositoryFactory);

        var miage1 = await createParcours.ExecuteAsync("MIAGE", 1);
        var miage2 = await createParcours.ExecuteAsync("MIAGE", 2);
        parcours.Add(miage1);
        parcours.Add(miage2);

        var ueWeb = await createUe.ExecuteAsync("UE101", "Architecture web");
        var ueSi = await createUe.ExecuteAsync("UE102", "Architecture SI");
        var ueData = await createUe.ExecuteAsync("UE103", "Bases de donnees");
        ues.AddRange(new[] { ueWeb, ueSi, ueData });

        await addUeDansParcours.ExecuteAsync(miage1.Id, new[] { ueWeb.Id, ueSi.Id });
        await addUeDansParcours.ExecuteAsync(miage2.Id, new[] { ueData.Id });

        etudiants.Add(await createEtudiant.ExecuteAsync("E001", "Durand", "Alice", "alice.durand@etud.u-picardie.fr"));
        etudiants.Add(await createEtudiant.ExecuteAsync("E002", "Martin", "Leo", "leo.martin@etud.u-picardie.fr"));
        etudiants.Add(await createEtudiant.ExecuteAsync("E003", "Bernard", "Nina", "nina.bernard@etud.u-picardie.fr"));

        await addEtudiantDansParcours.ExecuteAsync(miage1.Id, new[] { etudiants[0].Id, etudiants[1].Id });
        await addEtudiantDansParcours.ExecuteAsync(miage2.Id, etudiants[2].Id);

        await RepositoryFactory.NoteRepository().CreateAsync(new Note
        {
            EtudiantId = etudiants[0].Id,
            UeId = ueWeb.Id,
            Valeur = 14.5m
        });
        await RepositoryFactory.NoteRepository().CreateAsync(new Note
        {
            EtudiantId = etudiants[1].Id,
            UeId = ueWeb.Id,
            Valeur = 12.0m
        });
        await RepositoryFactory.NoteRepository().CreateAsync(new Note
        {
            EtudiantId = etudiants[2].Id,
            UeId = ueData.Id,
            Valeur = 16.0m
        });
        await RepositoryFactory.NoteRepository().SaveChangesAsync();
    }

    protected override async Task BuildSecuriteAsync()
    {
        var createRole = new CreateUniversiteRoleUseCase(RepositoryFactory);
        var createUser = new CreateUniversiteUserUseCase(RepositoryFactory);

        await createRole.ExecuteAsync(Roles.Responsable);
        await createRole.ExecuteAsync(Roles.Scolarite);
        await createRole.ExecuteAsync(Roles.Etudiant);

        await createUser.ExecuteAsync("scolarite@universite.local", "Scolarite123!", Roles.Scolarite);
        await createUser.ExecuteAsync("responsable@universite.local", "Responsable123!", Roles.Responsable);

        foreach (var etudiant in etudiants)
        {
            await createUser.ExecuteAsync(
                etudiant.Email,
                "Etudiant123!",
                Roles.Etudiant,
                etudiant.Id);
        }
    }
}
