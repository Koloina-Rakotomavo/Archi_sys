namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class DuplicateIDParcourException : Exception
{
    public DuplicateIDParcourException() { }
    public DuplicateIDParcourException(string message) : base(message) { }
    public DuplicateIDParcourException(string message, Exception innerException) : base(message, innerException) { }
}
