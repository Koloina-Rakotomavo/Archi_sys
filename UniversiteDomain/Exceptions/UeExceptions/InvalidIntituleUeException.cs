namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class InvalidIntituleUeException : Exception
{
    public InvalidIntituleUeException()
        : base("Intitulé d’UE invalide.") { }

    public InvalidIntituleUeException(string intitule)
        : base($"Intitulé d'UE invalide : '{intitule}'. Il doit contenir au moins 3 caractères.") { }

    public InvalidIntituleUeException(string message, Exception inner)
        : base(message, inner) { }
}
