using UniversiteDomain.Entities;

namespace UniversiteDomain.Dtos.Etudiants;

public class ParcoursSummaryDto
{
    public long Id { get; set; }
    public string NomParcours { get; set; } = string.Empty;
    public int AnneeFormation { get; set; }

    public static ParcoursSummaryDto FromEntity(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        return new ParcoursSummaryDto
        {
            Id = parcours.Id,
            NomParcours = parcours.NomParcours,
            AnneeFormation = parcours.AnneeFormation
        };
    }
}
