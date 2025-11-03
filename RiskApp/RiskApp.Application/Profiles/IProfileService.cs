namespace RiskApp.Application.Profiles;

public interface IProfileService
{
    Task<ProfileReadDto> CreateAsync(ProfileCreateDto dto, CancellationToken ct = default);
    Task<ProfileReadDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProfileReadDto>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, ProfileUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
