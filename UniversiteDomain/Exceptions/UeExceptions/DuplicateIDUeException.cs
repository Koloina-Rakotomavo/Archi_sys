namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class DuplicateIDUeException : Exception
{
    public DuplicateIDUeException() { }
    public DuplicateIDUeException(string message) : base(message) { }
    public DuplicateIDUeException(string message, Exception innerException) : base(message, innerException) { }
}
