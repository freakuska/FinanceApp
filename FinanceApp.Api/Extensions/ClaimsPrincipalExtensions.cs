using System.Security.Claims;

namespace FinanceApp.Api.Extensions;

/// <summary>
/// Extension методы для работы с ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Получить UserId из токена
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst("userId")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Получить Email из токена
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Получить имя пользователя из токена
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Получить список ролей пользователя
    /// </summary>
    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Проверить наличие роли у пользователя
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }

    /// <summary>
    /// Получить список permissions пользователя
    /// </summary>
    public static List<string> GetPermissions(this ClaimsPrincipal principal)
    {
        return principal.FindAll("permission")
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Проверить наличие permission у пользователя
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        var permissions = principal.GetPermissions();
        return permissions.Contains("*") || permissions.Contains(permission);
    }
}

