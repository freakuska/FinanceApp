using FinanceApp.Dbo.Enums;

namespace FinanceApp.Infrastructure.Dtos;

public record UserDto
{
    public Guid Id { get; init; }
    public string Login { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string Phone { get; init; }
    public string AvatarUrl { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public UserSettingsDto Settings { get; init; }
    public List<RoleDto> Roles { get; init; }
}

public record CreateUserDto
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string FullName { get; init; }
    public string Phone { get; init; }
}

public record UpdateUserDto
{
    public string FullName { get; init; }
    public string Phone { get; init; }
    public string AvatarUrl { get; init; }
    public UserSettingsDto Settings { get; init; }
}

public record ChangePasswordDto
{
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
}

public record UserSettingsDto
{
    public string Currency { get; init; } = "RUB";
    public string Language { get; init; } = "ru";
    public string Timezone { get; init; } = "UTC";
}

// ============================================
// Role DTOs
// ============================================
public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Code { get; init; }
    public string Description { get; init; }
    public List<string> Permissions { get; init; }
    public bool IsSystem { get; init; }
}

public record CreateRoleDto
{
    public string Name { get; init; }
    public string Code { get; init; }
    public string Description { get; init; }
    public List<string> Permissions { get; init; }
}

public record UpdateRoleDto
{
    public string Name { get; init; }
    public string Description { get; init; }
    public List<string> Permissions { get; init; }
}

// ============================================
// Tag DTOs
// ============================================
public record TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Type { get; init; }
    public string Icon { get; init; }
    public string Color { get; init; }
    public string Visibility { get; init; }
    public int? Level { get; init; }
    public int UsageCount { get; init; }
    public List<TagDto> Children { get; init; }
}

public record CreateTagDto
{
    public string Name { get; init; }
    public TagType Type { get; init; }
    public Guid? ParentId { get; init; }
    public string Icon { get; init; }
    public string Color { get; init; }
    public TagVisibility Visibility { get; init; } = TagVisibility.Private;
}

public record UpdateTagDto
{
    public string Name { get; init; }
    public string Icon { get; init; }
    public string Color { get; init; }
    public TagVisibility Visibility { get; init; }
    public int? SortOrder { get; init; }
}

// ============================================
// Operation DTOs
// ============================================
public record OperationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public MoneyDto Money { get; init; }
    public string PaymentMethod { get; init; }
    public string Description { get; init; }
    public string Notes { get; init; }
    public DateTime OperationDateTime { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<TagDto> Tags { get; init; }
}

public record MoneyDto
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
}

public record CreateOperationDto
{
    public OperationType Type { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "RUB";
    public PaymentMethod PaymentMethod { get; init; }
    public string Description { get; init; }
    public string Notes { get; init; }
    public DateTime? OperationDateTime { get; init; }
    public List<Guid> TagIds { get; init; }
}

public record UpdateOperationDto
{
    public decimal? Amount { get; init; }
    public string Currency { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public string Description { get; init; }
    public string Notes { get; init; }
    public DateTime? OperationDateTime { get; init; }
    public List<Guid> TagIds { get; init; }
}

public record OperationFilterDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public OperationType? Type { get; init; }
    public string? Currency { get; init; }
    public List<Guid>? TagIds { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record OperationStatsDto
{
    public string Currency { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Balance { get; init; }
    public int Count { get; init; }
}

// ============================================
// Report DTOs
// ============================================
public record MonthlyReportDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public Dictionary<string, OperationStatsDto> ByCurrency { get; init; }
    public List<CategoryStatsDto> ByCategory { get; init; }
    public List<DailyStatsDto> ByDay { get; init; }
}

public record YearlyReportDto
{
    public int Year { get; init; }
    public Dictionary<string, OperationStatsDto> ByCurrency { get; init; }
    public List<MonthlyStatsDto> ByMonth { get; init; }
}

public record CategoryReportDto
{
    public List<CategoryStatsDto> Categories { get; init; }
}

public record CategoryStatsDto
{
    public Guid TagId { get; init; }
    public string TagName { get; init; }
    public string TagIcon { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public int Count { get; init; }
    public decimal Percentage { get; set; }
}

public record DailyStatsDto
{
    public DateTime Date { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
}

public record MonthlyStatsDto
{
    public int Month { get; init; }
    public string MonthName { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
}

public record TrendReportDto
{
    public List<TrendDataDto> Data { get; init; }
    public string GroupBy { get; init; }
}

public record TrendDataDto
{
    public DateTime Date { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
}

public record PagedResult<T>
{
    public List<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ============================================
// Authentication DTOs
// ============================================
public record LoginRequestDto
{
    public string Email { get; init; }
    public string Password { get; init; }
}

public record RegisterRequestDto
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string FullName { get; init; }
    public string Phone { get; init; }
}

public record AuthResponseDto
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; }
}

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; init; }
}