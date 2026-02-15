namespace UniversiteDomain.Entities;

public class Ue
{
    public long Id { get; set; }

    // ✅ C’est bien CE nom-là qu’on garde partout
    public string NumeroUe { get; set; } = string.Empty;

    public string Intitule { get; set; } = string.Empty;

    // ManyToMany : une Ue est enseignée dans plusieurs parcours
    public List<Parcours>? EnseigneeDans { get; set; } = new();
    
    // OneToMany : une UE possède plusieurs notes
    public List<Note>? Notes { get; set; } = new();

    public override string ToString()
    {
        return $"ID {Id} : {NumeroUe} - {Intitule}";
    }
}
