namespace RiskApp.Application.Abstractions;

public interface IRepository<T> : IReadOnlyRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
