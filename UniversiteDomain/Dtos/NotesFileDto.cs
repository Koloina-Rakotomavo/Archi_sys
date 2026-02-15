namespace UniversiteDomain.Dtos;

public class NotesFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "notes.csv";
    public string ContentType { get; set; } = "text/csv";
}
