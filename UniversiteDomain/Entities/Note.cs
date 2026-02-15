namespace UniversiteDomain.Entities;

public class Note
{
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    public decimal? Valeur { get; set; }

    public Etudiant? Etudiant { get; set; }
    public Ue? Ue { get; set; }

    public override string ToString()
    {
        return $"Note etudiant={EtudiantId}, ue={UeId}, valeur={Valeur}";
    }
}
