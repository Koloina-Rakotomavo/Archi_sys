namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteEtudiantException : Exception
{
    public InvalidNoteEtudiantException() { }
    public InvalidNoteEtudiantException(string message) : base(message) { }
    public InvalidNoteEtudiantException(string message, Exception innerException) : base(message, innerException) { }
}
