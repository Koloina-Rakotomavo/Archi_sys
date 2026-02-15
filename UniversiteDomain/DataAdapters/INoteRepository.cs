using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<List<Note>> FindByUeAsync(long ueId);
    Task<NotesFileDto> WriteAsync(List<LigneSaisieDto> lignes, string fileNameBase);
    Task<List<LigneSaisieDto>> ReadAsync(byte[] content);
    Task UpsertManyAsync(List<Note> notes);
    Task DeleteManyAsync(List<(long EtudiantId, long UeId)> keys);
}
