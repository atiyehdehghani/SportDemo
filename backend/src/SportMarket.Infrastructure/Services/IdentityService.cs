using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Common.Models;
using SportMarket.Infrastructure.Identity;

namespace SportMarket.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<OperationResult> CreateUserAsync(string email, string password, string fullName, string role, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return ToResult(createResult);
        }

        await EnsureRoleExistsAsync(role);

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        return roleResult.Succeeded ? OperationResult.Success() : ToResult(roleResult);
    }

    public async Task<OperationResult> UpdateProfileAsync(string userId, string fullName, bool isActive, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failed("کاربر یافت نشد.");
        }

        user.FullName = fullName;
        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);
        return ToResult(result);
    }

    public async Task<OperationResult> UpdateRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failed("کاربر یافت نشد.");
        }

        await EnsureRoleExistsAsync(role);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(role))
        {
            return OperationResult.Success();
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return ToResult(removeResult);
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        return ToResult(addResult);
    }

    public async Task<OperationResult> SetPasswordAsync(string userId, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failed("کاربر یافت نشد.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        return ToResult(result);
    }

    public async Task<OperationResult> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failed("کاربر یافت نشد.");
        }

        var result = await _userManager.DeleteAsync(user);
        return ToResult(result);
    }

    public async Task<UserSnapshot?> GetSnapshotAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : await ToSnapshotAsync(user);
    }

    public async Task<UserSnapshot?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : await ToSnapshotAsync(user);
    }

    public async Task<(UserSnapshot User, IReadOnlyList<string> Roles)?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return null;
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var snapshot = new UserSnapshot
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = roles
        };

        return (snapshot, roles);
    }

    public async Task<PagedResult<UserSnapshot>> GetUsersAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.FullName.Contains(term) || u.Email!.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<UserSnapshot>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserSnapshot
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc,
                Roles = roles.ToList()
            });
        }

        return new PagedResult<UserSnapshot>(items, totalCount, page, pageSize);
    }

    public async Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        return await _userManager.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
    }

    private async Task<UserSnapshot> ToSnapshotAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserSnapshot
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = roles.ToList()
        };
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static OperationResult ToResult(IdentityResult result) =>
        result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failed(result.Errors.Select(e => e.Description).ToArray());
}
