using UniversiteDomain.Entities;

namespace UniversiteDomain.Dtos.Etudiants;

public class EtudiantDetailDto
{
    public long Id { get; set; }
    public string NumEtud { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ParcoursSummaryDto? Parcours { get; set; }
    public List<NoteDetailDto> Notes { get; set; } = new();

    public static EtudiantDetailDto FromEntity(Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(etudiant);

        return new EtudiantDetailDto
        {
            Id = etudiant.Id,
            NumEtud = etudiant.NumEtud,
            Nom = etudiant.Nom,
            Prenom = etudiant.Prenom,
            Email = etudiant.Email,
            Parcours = etudiant.ParcoursSuivi is null ? null : ParcoursSummaryDto.FromEntity(etudiant.ParcoursSuivi),
            Notes = etudiant.NotesObtenues?
                .OrderBy(n => n.Ue?.NumeroUe)
                .Select(NoteDetailDto.FromEntity)
                .ToList() ?? new List<NoteDetailDto>()
        };
    }
}
