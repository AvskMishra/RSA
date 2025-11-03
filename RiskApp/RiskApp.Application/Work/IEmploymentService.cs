namespace RiskApp.Application.Work;

public interface IEmploymentService
{
    Task<EmploymentReadDto> CreateAsync(EmploymentCreateDto dto, CancellationToken ct = default);
    Task<EmploymentReadDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EmploymentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, EmploymentUpdateDto dto, CancellationToken ct = default);
    Task<bool> CloseAsync(Guid id, EmploymentCloseDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
