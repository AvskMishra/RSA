using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Profiles;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Profiles;

public class ProfileService : IProfileService
{
    private readonly RiskAppDbContext _db;

    public ProfileService(RiskAppDbContext db) => _db = db;

    public async Task<ProfileReadDto> CreateAsync(ProfileCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Profile(dto.FullName, dto.DateOfBirth, dto.NationalId, dto.Email, dto.Phone, dto.Address);
        _db.Profiles.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<ProfileReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Profiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<ProfileReadDto>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _db.Profiles.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip).Take(take)
            .Select(x => new ProfileReadDto(x.Id, x.FullName, x.DateOfBirth, x.NationalId, x.Email, x.Phone, x.Address, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, ProfileUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Profiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.UpdateContact(dto.Email, dto.Phone, dto.Address);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Profiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.Profiles.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ProfileReadDto Map(Profile x) =>
        new(x.Id, x.FullName, x.DateOfBirth, x.NationalId, x.Email, x.Phone, x.Address, x.CreatedAtUtc, x.UpdatedAtUtc);
}
