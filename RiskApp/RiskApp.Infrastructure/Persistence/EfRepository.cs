using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Abstractions;

namespace RiskApp.Infrastructure.Persistence;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly RiskAppDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(RiskAppDbContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync(new object?[] { id }, ct);

    public async Task<IReadOnlyList<T>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
        => await _set.AsNoTracking().Skip(skip).Take(take).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Delete(T entity) => _set.Remove(entity);
}
