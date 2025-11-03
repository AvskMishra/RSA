using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Work;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Work;

public class EmploymentService : IEmploymentService
{
    private readonly RiskAppDbContext _db;
    public EmploymentService(RiskAppDbContext db) => _db = db;

    public async Task<EmploymentReadDto> CreateAsync(EmploymentCreateDto dto, CancellationToken ct = default)
    {
        // Guard: profile must exist (cheap check)
        var profileExists = await _db.Profiles.AsNoTracking().AnyAsync(p => p.Id == dto.ProfileId, ct);
        if (!profileExists) throw new InvalidOperationException("Profile not found.");

        var entity = new EmploymentRecord(dto.ProfileId, dto.EmployerName, dto.Type, dto.StartDate, dto.MonthlyIncome, dto.EndDate);
        _db.EmploymentRecords.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<EmploymentReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.EmploymentRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<IReadOnlyList<EmploymentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _db.EmploymentRecords.AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.IsCurrent).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take)
            .Select(x => new EmploymentReadDto
            {
                Id = x.Id,
                ProfileId = x.ProfileId,
                EmployerName = x.EmployerName,
                Type = x.Type,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                MonthlyIncome = x.MonthlyIncome,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, EmploymentUpdateDto dto, CancellationToken ct = default)
    {
        var e = await _db.EmploymentRecords.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        // Update changeable fields
        if (!string.IsNullOrWhiteSpace(dto.EmployerName))
        {
            // tiny setter via reflection avoidance; entity keeps private setter—so reassign
            typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.EmployerName))!
                .SetValue(e, dto.EmployerName.Trim());
        }

        typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.MonthlyIncome))!
            .SetValue(e, dto.MonthlyIncome);

        typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.Type))!
            .SetValue(e, dto.Type);

        e.Touch();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CloseAsync(Guid id, EmploymentCloseDto dto, CancellationToken ct = default)
    {
        var e = await _db.EmploymentRecords.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        e.Close(dto.EndDate);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.EmploymentRecords.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        _db.EmploymentRecords.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static EmploymentReadDto Map(EmploymentRecord x) => new()
    {
        Id = x.Id,
        ProfileId = x.ProfileId,
        EmployerName = x.EmployerName,
        Type = x.Type,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        IsCurrent = x.IsCurrent,
        MonthlyIncome = x.MonthlyIncome,
        CreatedAtUtc = x.CreatedAtUtc,
        UpdatedAtUtc = x.UpdatedAtUtc
    };
}
