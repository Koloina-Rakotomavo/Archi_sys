using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Entities;

namespace UniversiteEFDataProvider.Data;

public class UniversiteDbContext : IdentityDbContext<UniversiteUser, UniversiteRole, string>
{
    public UniversiteDbContext(DbContextOptions<UniversiteDbContext> options) : base(options)
    {
    }

    public UniversiteDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Etudiant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumEtud).IsRequired().HasMaxLength(32);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Prenom).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => e.NumEtud).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.ParcoursSuivi)
                .WithMany(p => p.Inscrits)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.NotesObtenues)
                .WithOne(n => n.Etudiant)
                .HasForeignKey(n => n.EtudiantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<UniversiteUser>()
                .WithOne(u => u.EtudiantLie)
                .HasForeignKey<UniversiteUser>(u => u.EtudiantLieId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Parcours>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.NomParcours).IsRequired().HasMaxLength(200);
            entity.Property(p => p.AnneeFormation).IsRequired();
            entity.HasIndex(p => new { p.NomParcours, p.AnneeFormation }).IsUnique();
        });

        modelBuilder.Entity<Ue>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NumeroUe).IsRequired().HasMaxLength(32);
            entity.Property(u => u.Intitule).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.NumeroUe).IsUnique();

            entity.HasMany(u => u.Notes)
                .WithOne(n => n.Ue)
                .HasForeignKey(n => n.UeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(n => new { n.EtudiantId, n.UeId });
            entity.Property(n => n.Valeur).HasPrecision(4, 2);
        });

        modelBuilder.Entity<Parcours>()
            .HasMany(p => p.UesEnseignees)
            .WithMany(u => u.EnseigneeDans)
            .UsingEntity<Dictionary<string, object>>(
                "ParcoursUe",
                right => right.HasOne<Ue>().WithMany().HasForeignKey("UeId"),
                left => left.HasOne<Parcours>().WithMany().HasForeignKey("ParcoursId"),
                join =>
                {
                    join.IndexerProperty<long>("ParcoursId");
                    join.IndexerProperty<long>("UeId");
                    join.HasKey("ParcoursId", "UeId");
                });
    }

    public DbSet<Etudiant> Etudiants => Set<Etudiant>();
    public DbSet<Parcours> Parcours => Set<Parcours>();
    public DbSet<Ue> Ues => Set<Ue>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<UniversiteRole> UniversiteRoles => Set<UniversiteRole>();
    public DbSet<UniversiteUser> UniversiteUsers => Set<UniversiteUser>();
}
