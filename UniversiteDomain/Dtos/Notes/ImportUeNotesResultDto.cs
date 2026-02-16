namespace UniversiteDomain.Dtos.Notes;

public class ImportUeNotesResultDto
{
    public int RowsRead { get; set; }
    public int UpsertedCount { get; set; }
    public int DeletedCount { get; set; }
}
