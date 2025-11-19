using System.Linq.Expressions;
using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUeRepository
{
    Task<Ue> CreateAsync(Ue ue);
    Task<List<Ue>> FindByConditionAsync(Expression<Func<Ue, bool>> expression);
    Task SaveChangesAsync();
}