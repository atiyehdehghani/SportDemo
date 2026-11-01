namespace SportMarket.Application.Common.Models;

public class UserSnapshot
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
