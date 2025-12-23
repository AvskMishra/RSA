namespace RiskApp.Application.Profiles;

public record ProfileCreateDto(
 string FullName,
 DateOnly DateOfBirth,
 string NationalId,
 string? Email,
 string? Phone,
 string? Address
);

public record ProfileReadDto(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string NationalId,
    string? Email,
    string? Phone,
    string? Address,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
public record ProfileUpdateDto(
    string? Email,
    string? Phone,
    string? Address
);