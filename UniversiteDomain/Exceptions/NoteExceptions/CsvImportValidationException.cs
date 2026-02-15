namespace UniversiteDomain.Exceptions.NoteExceptions;

public class CsvImportValidationException(List<string> errors) : Exception("Le fichier CSV contient des erreurs.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
