namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class InvalidAnneeParcoursException : Exception
{
    public InvalidAnneeParcoursException()
        : base("Année de formation invalide.") { }

    public InvalidAnneeParcoursException(int annee)
        : base($"L'année de formation '{annee}' est invalide.") { }

    public InvalidAnneeParcoursException(int annee, string message)
        : base(message + $" Valeur reçue : {annee}.") { }
}