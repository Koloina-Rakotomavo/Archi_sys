namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class InvalidNomParcoursException : Exception
{
    public InvalidNomParcoursException()
        : base("Nom de parcours invalide.") { }

  
    public InvalidNomParcoursException(string message, Exception inner)
        : base(message, inner) { }
}