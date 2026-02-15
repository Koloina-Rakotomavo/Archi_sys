namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteException : Exception
{
    public InvalidNoteException() { }
    public InvalidNoteException(string message) : base(message) { }
    public InvalidNoteException(string message, Exception innerException) : base(message, innerException) { }
}
