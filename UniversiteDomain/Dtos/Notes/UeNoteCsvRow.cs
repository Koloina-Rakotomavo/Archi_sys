namespace UniversiteDomain.Dtos.Notes;

public class UeNoteCsvRow
{
    public string NumeroUe { get; set; } = string.Empty;
    public string IntituleUe { get; set; } = string.Empty;
    public string NumEtud { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public decimal? Note { get; set; }
}
