namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class DuplicateNumeroUeException : Exception
{
    public DuplicateNumeroUeException()
        : base("Une UE avec ce numéro existe déjà.") { }

    public DuplicateNumeroUeException(string numeroUe)
        : base($"L’UE avec le numéro '{numeroUe}' existe déjà.") { }

    public DuplicateNumeroUeException(string message, Exception inner)
        : base(message, inner) { }
}
