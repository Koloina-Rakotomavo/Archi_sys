using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class UeRepository(UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task<Ue?> FindByNumeroAsync(string numeroUe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroUe);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        var numero = numeroUe.Trim();
        return await Context.Ues.FirstOrDefaultAsync(u => u.NumeroUe == numero);
    }
}
