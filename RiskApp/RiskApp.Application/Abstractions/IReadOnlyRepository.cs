namespace RiskApp.Application.Abstractions;

public interface IReadOnlyRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);
}
