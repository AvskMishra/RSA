using RiskApp.Application.Abstractions;

namespace RiskApp.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly RiskAppDbContext _db;
    public UnitOfWork(RiskAppDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
