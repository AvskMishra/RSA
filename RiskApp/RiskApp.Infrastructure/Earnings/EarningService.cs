using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiskApp.Application.Abstractions;
using RiskApp.Application.Earnings;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Earnings;

public class EarningService : IEarningService
{
    private readonly IRepository<Earning> _repo;
    private readonly IReadOnlyRepository<Profile> _profiles;
    private readonly RiskAppDbContext _db; // for projections
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EarningService> _logger;
    public EarningService(IRepository<Earning> repo,IReadOnlyRepository<Profile> profiles,
        RiskAppDbContext db,IUnitOfWork uow,ILogger<EarningService>logger)
    {
        _repo = repo;
        _profiles = profiles;
        _db = db;
        _uow = uow;
        _logger = logger;
    }

    public async Task<EarningReadDto> CreateAsync(EarningCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating earning record for ProfileId {ProfileId}", dto.ProfileId);
        var owner = await _profiles.GetByIdAsync(dto.ProfileId, ct);
        if (owner is null)
        {
            _logger.LogWarning("ProfileId {ProfileId} not found when creating earning record", dto.ProfileId);
            throw new InvalidOperationException("Profile not found.");
        }
        var entity = new Earning(dto.ProfileId, dto.MonthlyIncome, dto.OtherMonthlyIncome, dto.EffectiveFrom, dto.Currency);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Created earning record with ID {EarningId}", entity.Id);
        return Map(entity);
    }

    public async Task<EarningReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving earning record with ID {EarningId}", id);
        var e = await _db.Earnings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
        {
            _logger.LogInformation("Earning record with ID {EarningId} not found", id);
            return null;
        }
        else
        {
            _logger.LogInformation("Earning record with ID {EarningId} retrieved", id);
            return Map(e);
        }
    }

    public async Task<IReadOnlyList<EarningReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        _logger.LogInformation("Listing earning records for ProfileId {ProfileId}, skip {Skip}, take {Take}", profileId, skip, take);
        return await _db.Earnings.AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.EffectiveFrom)
            .Skip(skip).Take(take)
            .Select(x => Map(x))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, EarningUpdateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating earning record with ID {EarningId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Earning record with ID {EarningId} not found", id);
            return false;
        }

        typeof(Earning).GetProperty(nameof(Earning.MonthlyIncome))!.SetValue(e, dto.MonthlyIncome);
        typeof(Earning).GetProperty(nameof(Earning.OtherMonthlyIncome))!.SetValue(e, dto.OtherMonthlyIncome);

        if (!string.IsNullOrWhiteSpace(dto.Currency))
            typeof(Earning).GetProperty(nameof(Earning.Currency))!.SetValue(e, dto.Currency);

        if (dto.EffectiveFrom is not null)
            typeof(Earning).GetProperty(nameof(Earning.EffectiveFrom))!.SetValue(e, dto.EffectiveFrom.Value);

        e.Touch();
        _repo.Update(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Updated earning record with ID {EarningId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting earning record with ID {EarningId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Earning record with ID {EarningId} not found", id);
            return false;
        }
        _repo.Delete(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted earning record with ID {EarningId}", id);
        return true;
    }

    private static EarningReadDto Map(Earning x) => new()
    {
        Id = x.Id,
        ProfileId = x.ProfileId,
        MonthlyIncome = x.MonthlyIncome,
        OtherMonthlyIncome = x.OtherMonthlyIncome,
        Currency = x.Currency,
        EffectiveFrom = x.EffectiveFrom,
        CreatedAtUtc = x.CreatedAtUtc,
        UpdatedAtUtc = x.UpdatedAtUtc
    };
}
