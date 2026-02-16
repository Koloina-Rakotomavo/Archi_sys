using UniversiteDomain.Entities;

namespace UniversiteDomain.Dtos.Etudiants;

public class UeSummaryDto
{
    public long Id { get; set; }
    public string NumeroUe { get; set; } = string.Empty;
    public string Intitule { get; set; } = string.Empty;

    public static UeSummaryDto FromEntity(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        return new UeSummaryDto
        {
            Id = ue.Id,
            NumeroUe = ue.NumeroUe,
            Intitule = ue.Intitule
        };
    }
}
