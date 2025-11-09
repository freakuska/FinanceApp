using FinanceApp.Dbo.Enums;
using FinanceApp.Infrastructure.Dtos;

namespace FinanceApp.Infrastructure.Services;

// ============================================
// IUserService
// ============================================
public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto);
    Task<bool> VerifyEmailAsync(Guid id);
    Task<List<UserDto>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<bool> AssignRoleAsync(Guid userId, string roleCode, Guid assignedBy);
    Task<bool> RemoveRoleAsync(Guid userId, string roleCode);
    Task<List<RoleDto>> GetUserRolesAsync(Guid userId);
    Task UpdateLastLoginAsync(Guid userId);
}

// ============================================
// IRoleService
// ============================================
public interface IRoleService
{
    Task<RoleDto> GetByIdAsync(Guid id);
    Task<RoleDto> GetByCodeAsync(string code);
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<RoleDto>> GetAllAsync();
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}

// ============================================
// ITagService
// ============================================
public interface ITagService
{
    Task<TagDto> GetByIdAsync(Guid id);
    Task<TagDto> GetBySlugAsync(string slug);
    Task<TagDto> CreateAsync(Guid userId, CreateTagDto dto);
    Task<TagDto> UpdateAsync(Guid id, UpdateTagDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<TagDto>> GetByTypeAsync(TagType type, Guid? userId = null);
    Task<List<TagDto>> GetTreeAsync(TagType? type = null, Guid? userId = null);
    Task<List<TagDto>> GetPopularAsync(int count = 10);
    Task<List<TagDto>> SearchAsync(string query, Guid? userId = null);
    Task IncrementUsageAsync(Guid tagId);
    Task<bool> ChangeVisibilityAsync(Guid tagId, TagVisibility visibility, Guid userId);
}

// ============================================
// IFinancialOperationService
// ============================================
public interface IFinancialOperationService
{
    Task<OperationDto> GetByIdAsync(Guid id, Guid userId);
    Task<OperationDto> CreateAsync(CreateOperationDto dto, Guid userId);
    Task<OperationDto> UpdateAsync(Guid id, UpdateOperationDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<bool> RestoreAsync(Guid id, Guid userId);
    Task<List<OperationDto>> GetUserOperationsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        OperationType? type = null,
        string currency = null,
        List<Guid> tagIds = null,
        int page = 1,
        int pageSize = 50);
    Task<PagedResult<OperationDto>> GetPagedAsync(
        Guid userId,
        OperationFilterDto filter);
    Task<Dictionary<string, OperationStatsDto>> GetStatsByCurrencyAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);
}

// ============================================
// IReportService
// ============================================
public interface IReportService
{
    Task<MonthlyReportDto> GetMonthlyReportAsync(Guid userId, int year, int month);
    Task<YearlyReportDto> GetYearlyReportAsync(Guid userId, int year);
    Task<CategoryReportDto> GetCategoryReportAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);
    Task<TrendReportDto> GetTrendReportAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string groupBy = "day"); // day, week, month
    Task<byte[]> ExportToCsvAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<byte[]> ExportToExcelAsync(Guid userId, DateTime startDate, DateTime endDate);
}

// ============================================
// IAuthService
// ============================================
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}