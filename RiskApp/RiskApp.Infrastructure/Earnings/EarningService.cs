using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Earnings;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Earnings;

public class EarningService : IEarningService
{
    private readonly RiskAppDbContext _db;
    public EarningService(RiskAppDbContext db) => _db = db;

    public async Task<EarningReadDto> CreateAsync(EarningCreateDto dto, CancellationToken ct = default)
    {
        var profileExists = await _db.Profiles.AsNoTracking().AnyAsync(p => p.Id == dto.ProfileId, ct);
        if (!profileExists) throw new InvalidOperationException("Profile not found.");

        var entity = new Earning(dto.ProfileId, dto.MonthlyIncome, dto.OtherMonthlyIncome, dto.EffectiveFrom, dto.Currency);
        _db.Earnings.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<EarningReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Earnings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<IReadOnlyList<EarningReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _db.Earnings.AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.EffectiveFrom)
            .Skip(skip).Take(take)
            .Select(x => new EarningReadDto
            {
                Id = x.Id,
                ProfileId = x.ProfileId,
                MonthlyIncome = x.MonthlyIncome,
                OtherMonthlyIncome = x.OtherMonthlyIncome,
                Currency = x.Currency,
                EffectiveFrom = x.EffectiveFrom,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, EarningUpdateDto dto, CancellationToken ct = default)
    {
        var e = await _db.Earnings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        // Apply changes (entity has private setters; use reflection or add methods)
        typeof(Earning).GetProperty(nameof(Earning.MonthlyIncome))!.SetValue(e, dto.MonthlyIncome);
        typeof(Earning).GetProperty(nameof(Earning.OtherMonthlyIncome))!.SetValue(e, dto.OtherMonthlyIncome);

        if (!string.IsNullOrWhiteSpace(dto.Currency))
            typeof(Earning).GetProperty(nameof(Earning.Currency))!.SetValue(e, dto.Currency);

        if (dto.EffectiveFrom is not null)
            typeof(Earning).GetProperty(nameof(Earning.EffectiveFrom))!.SetValue(e, dto.EffectiveFrom.Value);

        e.Touch();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Earnings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        _db.Earnings.Remove(e);
        await _db.SaveChangesAsync(ct);
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
