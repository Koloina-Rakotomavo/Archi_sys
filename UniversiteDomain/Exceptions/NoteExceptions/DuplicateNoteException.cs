namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class DuplicateNoteException : Exception
{
    public DuplicateNoteException()
        : base("Une note existe deja pour cet etudiant et cette UE.") { }

    public DuplicateNoteException(string message)
        : base(message) { }

    public DuplicateNoteException(string message, Exception innerException)
        : base(message, innerException) { }
}
