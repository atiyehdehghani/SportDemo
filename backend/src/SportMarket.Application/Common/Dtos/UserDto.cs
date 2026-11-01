namespace SportMarket.Application.Common.Dtos;

public record UserDto(
    string Id,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc);
