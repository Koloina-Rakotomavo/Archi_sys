namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class ParcoursNotFoundException : Exception
{
    public ParcoursNotFoundException() 
        : base("Le parcours demandé est introuvable.") { }

    public ParcoursNotFoundException(string id) 
        : base($"Aucun parcours trouvé avec l'id {id}.") { }

    public ParcoursNotFoundException(string message, Exception inner) 
        : base(message, inner) { }
}