using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiskApp.Application.Abstractions;
using RiskApp.Application.Work;
using RiskApp.Domain.Entities;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Work;

public class EmploymentService : IEmploymentService
{
    private readonly IRepository<EmploymentRecord> _repo;
    private readonly IReadOnlyRepository<Profile> _profiles;
    private readonly RiskAppDbContext _db; // for projections
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EmploymentService> _logger;

    public EmploymentService(IRepository<EmploymentRecord> repo, IReadOnlyRepository<Profile> profiles,
        RiskAppDbContext db, IUnitOfWork uow, ILogger<EmploymentService> logger)
    {
        _repo = repo;
        _profiles = profiles;
        _db = db;
        _uow = uow;
        _logger = logger;
    }

    public async Task<EmploymentReadDto> CreateAsync(EmploymentCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating employment record for ProfileId {ProfileId}", dto.ProfileId);
        var owner = await _profiles.GetByIdAsync(dto.ProfileId, ct);
        if (owner is null)
        {
            _logger.LogWarning("ProfileId {ProfileId} not found when creating employment record", dto.ProfileId);
            throw new InvalidOperationException("Profile not found.");
        }
        var entity = new EmploymentRecord(dto.ProfileId, dto.EmployerName, dto.Type, dto.StartDate, dto.MonthlyIncome, dto.EndDate);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Created employment record with ID {EmploymentId}", entity.Id);
        return Map(entity);
    }

    public async Task<EmploymentReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving employment record with ID {EmploymentId}", id);
        var e = await _db.EmploymentRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
        {
            _logger.LogInformation("Employment record with ID {EmploymentId} not found", id);
            return null;
        }
        else
        {
            _logger.LogInformation("Employment record with ID {EmploymentId} retrieved", id);
            return Map(e);
        }
    }

    public async Task<IReadOnlyList<EmploymentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        _logger.LogInformation("Listing employment records for ProfileId {ProfileId}, skip {Skip}, take {Take}", profileId, skip, take);
        return await _db.EmploymentRecords.AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.IsCurrent).ThenByDescending(x => x.StartDate)
            .Skip(skip).Take(take)
            .Select(x => Map(x))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, EmploymentUpdateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating employment record with ID {EmploymentId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Employment record with ID {EmploymentId} not found for update", id);
            return false;
        }

        // Prefer domain methods; using reflection avoided earlier—let's add minimal mutation here:
        typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.MonthlyIncome))!.SetValue(e, dto.MonthlyIncome);
        typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.Type))!.SetValue(e, dto.Type);

        if (!string.IsNullOrWhiteSpace(dto.EmployerName))
            typeof(EmploymentRecord).GetProperty(nameof(EmploymentRecord.EmployerName))!.SetValue(e, dto.EmployerName.Trim());

        e.Touch();
        _repo.Update(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Updated employment record with ID {EmploymentId}", id);
        return true;
    }

    public async Task<bool> CloseAsync(Guid id, EmploymentCloseDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Closing employment record with ID {EmploymentId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Employment record with ID {EmploymentId} not found for closing", id);
            return false;
        }
        e.Close(dto.EndDate);
        _repo.Update(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Closed employment record with ID {EmploymentId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting employment record with ID {EmploymentId}", id);
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null)
        {
            _logger.LogInformation("Employment record with ID {EmploymentId} not found for deletion", id);
            return false;
        }
        _repo.Delete(e);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted employment record with ID {EmploymentId}", id);
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
