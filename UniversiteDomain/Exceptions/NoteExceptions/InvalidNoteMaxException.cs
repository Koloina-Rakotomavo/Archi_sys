namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteMaxException : Exception
{
    public InvalidNoteMaxException() { }
    public InvalidNoteMaxException(string message) : base(message) { }
    public InvalidNoteMaxException(string message, Exception innerException) : base(message, innerException) { }
}
