using UniversiteDomain.Entities;

namespace UniversiteDomain.Dtos.Etudiants;

public class NoteDetailDto
{
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    public decimal? Valeur { get; set; }
    public UeSummaryDto? Ue { get; set; }

    public static NoteDetailDto FromEntity(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new NoteDetailDto
        {
            EtudiantId = note.EtudiantId,
            UeId = note.UeId,
            Valeur = note.Valeur,
            Ue = note.Ue is null ? null : UeSummaryDto.FromEntity(note.Ue)
        };
    }
}
