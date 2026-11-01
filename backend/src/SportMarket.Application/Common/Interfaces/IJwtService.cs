namespace SportMarket.Application.Common.Interfaces;

public interface IJwtService
{
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(
        string userId, string email, string fullName, IEnumerable<string> roles);
}
