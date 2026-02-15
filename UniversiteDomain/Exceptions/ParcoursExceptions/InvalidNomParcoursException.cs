namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class InvalidNomParcoursException : Exception
{
    public InvalidNomParcoursException()
        : base("Nom de parcours invalide.") { }

    public InvalidNomParcoursException(string nom)
        : base($"Nom de parcours invalide : '{nom}'. Il doit contenir au moins 3 caracteres.") { }

  
    public InvalidNomParcoursException(string message, Exception inner)
        : base(message, inner) { }
}
