namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class DuplicateNomParcoursException : Exception
{
    public DuplicateNomParcoursException()
        : base("Un parcours avec ce nom existe déjà.") { }

    public DuplicateNomParcoursException(string nom)
        : base($"Le parcours '{nom}' existe déjà.") { }

    public DuplicateNomParcoursException(string message, Exception inner)
        : base(message, inner) { }
}