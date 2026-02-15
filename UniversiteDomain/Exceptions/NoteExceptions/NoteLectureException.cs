namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class NoteLectureException : Exception
{
    public NoteLectureException() { }
    public NoteLectureException(string message) : base(message) { }
    public NoteLectureException(string message, Exception innerException) : base(message, innerException) { }
}
