namespace RiskApp.Application.Earnings;

public interface IEarningService
{
    Task<EarningReadDto> CreateAsync(EarningCreateDto dto, CancellationToken ct = default);
    Task<EarningReadDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EarningReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, EarningUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
