using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiskApp.Application.Abstractions;
using RiskApp.Application.Profiles;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Profiles;

public class ProfileService : IProfileService
{
    private readonly IRepository<Profile> _repo;
    private readonly RiskAppDbContext _db; // only for AsNoTracking projections
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(IRepository<Profile> repo, RiskAppDbContext db, IUnitOfWork uow, ILogger<ProfileService> logger)
    {
        _repo = repo;
        _db = db;
        _uow = uow;
        _logger = logger;
    }

    public async Task<ProfileReadDto> CreateAsync(ProfileCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating profile for {FullName}", dto.FullName);
        var entity = new Profile(dto.FullName, dto.DateOfBirth, dto.NationalId, dto.Email, dto.Phone, dto.Address);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Created profile with ID {ProfileId}", entity.Id);
        return Map(entity);
    }

    public async Task<ProfileReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving profile with ID {ProfileId}", id);
        var e = await _db.Profiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        _logger.LogInformation(e is null ? "Profile with ID {ProfileId} not found" : "Profile with ID {ProfileId} retrieved", id);
        return e is null ? null : Map(e);
    }

    public async Task<IReadOnlyList<ProfileReadDto>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
    {
        _logger.LogInformation("Listing profiles, skip {Skip}, take {Take}", skip, take);
        return await _db.Profiles.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip).Take(take)
            .Select(x => Map(x))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, ProfileUpdateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating profile with ID {ProfileId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Profile with ID {ProfileId} found, updating contact info", id);
            return false;
        }
        e.UpdateContact(dto.Email, dto.Phone, dto.Address);
        _repo.Update(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Profile with ID {ProfileId} updated", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting profile with ID {ProfileId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Profile with ID {ProfileId} not found", id);
            return false;
        }
        _repo.Delete(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Profile with ID {ProfileId} deleted", id);
        return true;
    }

    private static ProfileReadDto Map(Profile x) =>
        new(x.Id, x.FullName, x.DateOfBirth, x.NationalId, x.Email, x.Phone, x.Address, x.CreatedAtUtc, x.UpdatedAtUtc);
}
