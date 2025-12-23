namespace RiskApp.Application.Risk;
public interface IRiskAssessmentService
{
    Task<RiskAssessmentReadDto> AssessAsync(RiskAssessRequestDto request, CancellationToken ct = default);
    Task<RiskAssessmentReadDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RiskAssessmentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default);
}
