namespace SportMarket.Application.Common.Interfaces;

/// <summary>عملیات مدیریت کاربران (Identity) — پیاده‌سازی در Infrastructure</summary>
public interface IIdentityService
{
    Task<OperationResult> CreateUserAsync(string email, string password, string fullName, string role, CancellationToken ct = default);

    Task<OperationResult> UpdateProfileAsync(string userId, string fullName, bool isActive, CancellationToken ct = default);

    Task<OperationResult> UpdateRoleAsync(string userId, string role, CancellationToken ct = default);

    Task<OperationResult> SetPasswordAsync(string userId, string password, CancellationToken ct = default);

    Task<OperationResult> DeleteUserAsync(string userId, CancellationToken ct = default);

    Task<UserSnapshot?> GetSnapshotAsync(string userId, CancellationToken ct = default);

    Task<UserSnapshot?> FindByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>ورود: در صورت موفقیت اطلاعات کاربر و نقش‌ها، در غیر این صورت null</summary>
    Task<(UserSnapshot User, IReadOnlyList<string> Roles)?> AuthenticateAsync(string email, string password, CancellationToken ct = default);

    Task<PagedResult<UserSnapshot>> GetUsersAsync(string? search, int page, int pageSize, CancellationToken ct = default);

    Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds, CancellationToken ct = default);
}
