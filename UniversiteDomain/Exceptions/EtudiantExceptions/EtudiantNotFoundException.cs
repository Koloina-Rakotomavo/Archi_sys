namespace UniversiteDomain.Exceptions.EtudiantExceptions;

[Serializable]
public class EtudiantNotFoundException : Exception
{
    public EtudiantNotFoundException() 
        : base("L'étudiant demandé est introuvable.") { }

    public EtudiantNotFoundException(string id) 
        : base($"Aucun étudiant trouvé avec l'id {id}.") { }

    public EtudiantNotFoundException(string message, Exception inner) 
        : base(message, inner) { }
}