using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinanceApp.Dbo.Enums;
using FinanceApp.Dbo.Models;
using FinanceApp.Infrastructure.Data;
using FinanceApp.Infrastructure.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Infrastructure.Services;

// ============================================
// UserService
// ============================================
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Login = dto.Email,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            FullName = dto.FullName,
            Phone = dto.Phone,
            IsActive = true,
            IsVerified = false,
            AvatarUrl = String.Empty,
            Settings = JsonSerializer.Serialize(new UserSettingsDto())
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Назначаем роль USER по умолчанию
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "USER");
        if (userRole != null)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id,
                AssignedBy = user.Id
            });
            await _context.SaveChangesAsync();
        }

        return await GetByIdAsync(user.Id);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.FullName = dto.FullName ?? user.FullName;
        user.Phone = dto.Phone ?? user.Phone;
        user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;

        if (dto.Settings != null)
            user.Settings = JsonSerializer.Serialize(dto.Settings);

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyEmailAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleCode, Guid assignedBy)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == roleCode);
        if (role == null) return false;

        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (exists) return false;

        _context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = role.Id,
            AssignedBy = assignedBy
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleCode)
    {
        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Role.Code == roleCode);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var roles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            Permissions = JsonSerializer.Deserialize<List<string>>(r.Permissions),
            IsSystem = r.IsSystem
        }).ToList();
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private UserDto MapToDto(User user)
    {
        var settings = string.IsNullOrEmpty(user.Settings)
            ? new UserSettingsDto()
            : JsonSerializer.Deserialize<UserSettingsDto>(user.Settings);

        return new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Settings = settings,
            Roles = user.UserRoles?.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Code = ur.Role.Code,
                Description = ur.Role.Description,
                Permissions = JsonSerializer.Deserialize<List<string>>(ur.Role.Permissions),
                IsSystem = ur.Role.IsSystem
            }).ToList() ?? new List<RoleDto>()
        };
    }

    private string HashPassword(string password)
    {
        // TODO: Используйте BCrypt или Identity
        return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password)));
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

// ============================================
// RoleService
// ============================================
public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> GetByIdAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> GetByCodeAsync(string code)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == code);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Code == dto.Code);
        if (exists)
            throw new InvalidOperationException("Role with this code already exists");

        var role = new Role
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            Permissions = JsonSerializer.Serialize(dto.Permissions ?? new List<string>()),
            IsSystem = false
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            throw new KeyNotFoundException("Role not found");

        if (role.IsSystem)
            throw new InvalidOperationException("Cannot modify system role");

        role.Name = dto.Name ?? role.Name;
        role.Description = dto.Description ?? role.Description;

        if (dto.Permissions != null)
            role.Permissions = JsonSerializer.Serialize(dto.Permissions);

        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null || role.IsSystem) return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _context.Roles.ToListAsync();
        return roles.Select(MapToDto).ToList();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        foreach (var userRole in userRoles)
        {
            var permissions = JsonSerializer.Deserialize<List<string>>(userRole.Role.Permissions);
            if (permissions.Contains("*") || permissions.Contains(permission))
                return true;
        }

        return false;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var allPermissions = new HashSet<string>();

        foreach (var userRole in userRoles)
        {
            var permissions = JsonSerializer.Deserialize<List<string>>(userRole.Role.Permissions);
            foreach (var perm in permissions)
                allPermissions.Add(perm);
        }

        return allPermissions.ToList();
    }

    private RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Code = role.Code,
            Description = role.Description,
            Permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions),
            IsSystem = role.IsSystem
        };
    }
}

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> GetByIdAsync(Guid id)
    {
        var tag = await _context.Tags
            .Include(t => t.ChildTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        return tag == null ? null : MapToDto(tag);
    }

    public async Task<TagDto> GetBySlugAsync(string slug)
    {
        var tag = await _context.Tags
            .Include(t => t.ChildTags)
            .FirstOrDefaultAsync(t => t.Slug == slug);

        return tag == null ? null : MapToDto(tag);
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto)
    {
        var slug = GenerateSlug(dto.Name);
        var exists = await _context.Tags.AnyAsync(t => t.Slug == slug);
        
        if (exists)
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        var level = 0;
        var path = slug;

        if (dto.ParentId.HasValue)
        {
            var parent = await _context.Tags.FindAsync(dto.ParentId.Value);
            if (parent != null)
            {
                level = (parent.Level ?? 0) + 1;
                path = $"{parent.Path}/{slug}";
            }
        }

        var tag = new Tag
        {
            Name = dto.Name,
            Slug = slug,
            Type = dto.Type,
            ParentId = dto.ParentId,
            Level = level,
            Path = path,
            Icon = dto.Icon,
            Color = dto.Color,
            Visibility = dto.Visibility,
            IsActive = true,
            IsSystem = false
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return MapToDto(tag);
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto dto)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found");

        if (tag.IsSystem)
            throw new InvalidOperationException("Cannot modify system tag");

        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != tag.Name)
        {
            tag.Name = dto.Name;
            tag.Slug = GenerateSlug(dto.Name);
        }

        tag.Icon = dto.Icon ?? tag.Icon;
        tag.Color = dto.Color ?? tag.Color;
        tag.Visibility = dto.Visibility;
        
        if (dto.SortOrder.HasValue)
            tag.SortOrder = dto.SortOrder.Value;

        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(tag);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null || tag.IsSystem) return false;

        // Проверяем есть ли дочерние теги
        var hasChildren = await _context.Tags.AnyAsync(t => t.ParentId == id);
        if (hasChildren)
            throw new InvalidOperationException("Cannot delete tag with children");

        tag.IsActive = false;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TagDto>> GetByTypeAsync(TagType type, Guid? userId = null)
    {
        var query = _context.Tags
            .Where(t => t.IsActive && t.Type == type);

        if (userId.HasValue)
        {
            query = query.Where(t =>
                t.Visibility == TagVisibility.Public ||
                t.OwnerId == userId.Value);
        }
        else
        {
            query = query.Where(t => t.Visibility == TagVisibility.Public);
        }

        var tags = await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return tags.Select(MapToDto).ToList();
    }

    public async Task<List<TagDto>> GetTreeAsync(TagType? type = null, Guid? userId = null)
    {
        var query = _context.Tags
            .Include(t => t.ChildTags)
            .Where(t => t.IsActive && t.ParentId == null);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (userId.HasValue)
        {
            query = query.Where(t =>
                t.Visibility == TagVisibility.Public ||
                t.OwnerId == userId.Value);
        }
        else
        {
            query = query.Where(t => t.Visibility == TagVisibility.Public);
        }

        var tags = await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return tags.Select(MapToDto).ToList();
    }

    public async Task<List<TagDto>> GetPopularAsync(int count = 10)
    {
        var tags = await _context.Tags
            .Where(t => t.IsActive && t.Visibility == TagVisibility.Public)
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync();

        return tags.Select(MapToDto).ToList();
    }

    public async Task<List<TagDto>> SearchAsync(string query, Guid? userId = null)
    {
        var searchQuery = _context.Tags
            .Where(t => t.IsActive && 
                (t.Name.Contains(query) || t.Slug.Contains(query)));

        if (userId.HasValue)
        {
            searchQuery = searchQuery.Where(t =>
                t.Visibility == TagVisibility.Public ||
                t.OwnerId == userId.Value);
        }
        else
        {
            searchQuery = searchQuery.Where(t => t.Visibility == TagVisibility.Public);
        }

        var tags = await searchQuery
            .Take(20)
            .ToListAsync();

        return tags.Select(MapToDto).ToList();
    }

    public async Task IncrementUsageAsync(Guid tagId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag != null)
        {
            tag.UsageCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ChangeVisibilityAsync(Guid tagId, TagVisibility visibility, Guid userId)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.OwnerId == userId);
        if (tag == null || tag.IsSystem) return false;

        tag.Visibility = visibility;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            Type = tag.Type.ToString(),
            Icon = tag.Icon,
            Color = tag.Color,
            Visibility = tag.Visibility.ToString(),
            Level = tag.Level,
            UsageCount = tag.UsageCount,
            Children = tag.ChildTags?.Select(MapToDto).ToList() ?? new List<TagDto>()
        };
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("ё", "e").Replace("а", "a").Replace("б", "b")
            .Replace("в", "v").Replace("г", "g").Replace("д", "d")
            .Replace("е", "e").Replace("ж", "zh").Replace("з", "z")
            .Replace("и", "i").Replace("й", "y").Replace("к", "k")
            .Replace("л", "l").Replace("м", "m").Replace("н", "n")
            .Replace("о", "o").Replace("п", "p").Replace("р", "r")
            .Replace("с", "s").Replace("т", "t").Replace("у", "u")
            .Replace("ф", "f").Replace("х", "h").Replace("ц", "ts")
            .Replace("ч", "ch").Replace("ш", "sh").Replace("щ", "sch")
            .Replace("ъ", "").Replace("ы", "y").Replace("ь", "")
            .Replace("э", "e").Replace("ю", "yu").Replace("я", "ya");

        return new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
    }
}

public class FinancialOperationService : IFinancialOperationService
{
    private readonly ApplicationDbContext _context;

    public FinancialOperationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationDto> GetByIdAsync(Guid id, Guid userId)
    {
        var operation = await _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == userId);

        return operation == null ? null : MapToDto(operation);
    }

    public async Task<OperationDto> CreateAsync(CreateOperationDto dto, Guid userId)
    {
        var money = new Money(dto.Amount, dto.Currency);

        var operation = new FinancialOperation
        {
            OwnerId = userId,
            CreatedBy = userId,
            Type = dto.Type,
            Money = money,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description,
            Notes = dto.Notes,
            OperationDateTime = dto.OperationDateTime ?? DateTime.UtcNow
        };

        _context.FinancialOperations.Add(operation);
        await _context.SaveChangesAsync();

        // Добавляем теги
        if (dto.TagIds != null && dto.TagIds.Any())
        {
            foreach (var tagId in dto.TagIds)
            {
                _context.OperationTags.Add(new OperationTag
                {
                    OperationId = operation.Id,
                    TagId = tagId
                });

                // Увеличиваем счетчик использования тега
                var tag = await _context.Tags.FindAsync(tagId);
                if (tag != null)
                {
                    tag.UsageCount++;
                }
            }
            await _context.SaveChangesAsync();
        }

        return await GetByIdAsync(operation.Id, userId);
    }

    public async Task<OperationDto> UpdateAsync(Guid id, UpdateOperationDto dto, Guid userId)
    {
        var operation = await _context.FinancialOperations
            .Include(o => o.OperationTags)
            .FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == userId);

        if (operation == null)
            throw new KeyNotFoundException("Operation not found");

        if (dto.Amount.HasValue || !string.IsNullOrEmpty(dto.Currency))
        {
            var amount = dto.Amount ?? operation.Money.Amount;
            var currency = dto.Currency ?? operation.Money.Currency;
            operation.Money = new Money(amount, currency);
        }

        if (dto.PaymentMethod.HasValue)
            operation.PaymentMethod = dto.PaymentMethod.Value;

        operation.Description = dto.Description ?? operation.Description;
        operation.Notes = dto.Notes ?? operation.Notes;

        if (dto.OperationDateTime.HasValue)
            operation.OperationDateTime = dto.OperationDateTime.Value;

        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        // Обновляем теги если указаны
        if (dto.TagIds != null)
        {
            // Удаляем старые связи
            var oldTags = operation.OperationTags.ToList();
            _context.OperationTags.RemoveRange(oldTags);

            // Добавляем новые
            foreach (var tagId in dto.TagIds)
            {
                _context.OperationTags.Add(new OperationTag
                {
                    OperationId = operation.Id,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id, userId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var operation = await _context.FinancialOperations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == userId);

        if (operation == null || operation.DeletedAt.HasValue) return false;

        operation.DeletedAt = DateTime.UtcNow;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id, Guid userId)
    {
        var operation = await _context.FinancialOperations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id && o.OwnerId == userId);

        if (operation == null || !operation.DeletedAt.HasValue) return false;

        operation.DeletedAt = null;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<OperationDto>> GetUserOperationsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        OperationType? type = null,
        string currency = null,
        List<Guid> tagIds = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .Where(o => o.OwnerId == userId);

        if (startDate.HasValue)
            query = query.Where(o => o.OperationDateTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OperationDateTime <= endDate.Value);

        if (type.HasValue)
            query = query.Where(o => o.Type == type.Value);

        if (!string.IsNullOrEmpty(currency))
            query = query.Where(o => o.Money.Currency == currency.ToUpper());

        if (tagIds != null && tagIds.Any())
        {
            query = query.Where(o => o.OperationTags
                .Any(ot => tagIds.Contains(ot.TagId)));
        }

        var operations = await query
            .OrderByDescending(o => o.OperationDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return operations.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<OperationDto>> GetPagedAsync(
        Guid userId,
        OperationFilterDto filter)
    {
        var query = _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .Where(o => o.OwnerId == userId);

        if (filter.StartDate.HasValue)
            query = query.Where(o => o.OperationDateTime >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(o => o.OperationDateTime <= filter.EndDate.Value);

        if (filter.Type.HasValue)
            query = query.Where(o => o.Type == filter.Type.Value);

        if (!string.IsNullOrEmpty(filter.Currency))
            query = query.Where(o => o.Money.Currency == filter.Currency.ToUpper());

        if (filter.TagIds != null && filter.TagIds.Any())
        {
            query = query.Where(o => o.OperationTags
                .Any(ot => filter.TagIds.Contains(ot.TagId)));
        }

        var totalCount = await query.CountAsync();

        var operations = await query
            .OrderByDescending(o => o.OperationDateTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<OperationDto>
        {
            Items = operations.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<Dictionary<string, OperationStatsDto>> GetStatsByCurrencyAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        var operations = await _context.FinancialOperations
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .ToListAsync();

        return operations
            .GroupBy(o => o.Money.Currency)
            .ToDictionary(
                g => g.Key,
                g => new OperationStatsDto
                {
                    Currency = g.Key,
                    TotalIncome = g
                        .Where(o => o.Type == OperationType.Income)
                        .Sum(o => o.Money.Amount),
                    TotalExpense = g
                        .Where(o => o.Type == OperationType.Expense)
                        .Sum(o => o.Money.Amount),
                    Balance = g
                        .Where(o => o.Type == OperationType.Income)
                        .Sum(o => o.Money.Amount) -
                        g.Where(o => o.Type == OperationType.Expense)
                        .Sum(o => o.Money.Amount),
                    Count = g.Count()
                });
    }

    private OperationDto MapToDto(FinancialOperation operation)
    {
        return new OperationDto
        {
            Id = operation.Id,
            Type = operation.Type.ToString(),
            Money = new MoneyDto
            {
                Amount = operation.Money.Amount,
                Currency = operation.Money.Currency
            },
            PaymentMethod = operation.PaymentMethod.ToString(),
            Description = operation.Description,
            Notes = operation.Notes,
            OperationDateTime = operation.OperationDateTime,
            CreatedAt = operation.CreatedAt,
            Tags = operation.OperationTags?
                .Select(ot => new TagDto
                {
                    Id = ot.Tag.Id,
                    Name = ot.Tag.Name,
                    Slug = ot.Tag.Slug,
                    Type = ot.Tag.Type.ToString(),
                    Icon = ot.Tag.Icon,
                    Color = ot.Tag.Color
                })
                .ToList() ?? new List<TagDto>()
        };
    }
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(Guid userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var operations = await _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .ToListAsync();

        // Группировка по валюте
        var byCurrency = operations
            .GroupBy(o => o.Money.Currency)
            .ToDictionary(
                g => g.Key,
                g => new OperationStatsDto
                {
                    Currency = g.Key,
                    TotalIncome = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                    TotalExpense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                    Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                             g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                    Count = g.Count()
                });

        // Группировка по категориям
        var byCategory = operations
            .Where(o => o.Type == OperationType.Expense)
            .SelectMany(o => o.OperationTags.Select(ot => new { Operation = o, Tag = ot.Tag }))
            .GroupBy(x => new { x.Tag.Id, x.Tag.Name, x.Tag.Icon, x.Operation.Money.Currency })
            .Select(g => new CategoryStatsDto
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                TagIcon = g.Key.Icon,
                Amount = g.Sum(x => x.Operation.Money.Amount),
                Currency = g.Key.Currency,
                Count = g.Count(),
                Percentage = 0 // Вычислим позже
            })
            .ToList();

        // Вычисляем проценты
        foreach (var categoryGroup in byCategory.GroupBy(c => c.Currency))
        {
            var total = categoryGroup.Sum(c => c.Amount);
            foreach (var category in categoryGroup)
            {
                category.Percentage = total > 0 ? (category.Amount / total) * 100 : 0;
            }
        }

        // Группировка по дням
        var byDay = operations
            .GroupBy(o => o.OperationDateTime.Date)
            .Select(g => new DailyStatsDto
            {
                Date = g.Key,
                Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                         g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            ByCurrency = byCurrency,
            ByCategory = byCategory,
            ByDay = byDay
        };
    }

    public async Task<YearlyReportDto> GetYearlyReportAsync(Guid userId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        var operations = await _context.FinancialOperations
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .ToListAsync();

        // Группировка по валюте
        var byCurrency = operations
            .GroupBy(o => o.Money.Currency)
            .ToDictionary(
                g => g.Key,
                g => new OperationStatsDto
                {
                    Currency = g.Key,
                    TotalIncome = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                    TotalExpense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                    Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                             g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                    Count = g.Count()
                });

        // Группировка по месяцам
        var byMonth = operations
            .GroupBy(o => o.OperationDateTime.Month)
            .Select(g => new MonthlyStatsDto
            {
                Month = g.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                         g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        return new YearlyReportDto
        {
            Year = year,
            ByCurrency = byCurrency,
            ByMonth = byMonth
        };
    }

    public async Task<CategoryReportDto> GetCategoryReportAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        var operations = await _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .ToListAsync();

        var categories = operations
            .SelectMany(o => o.OperationTags.Select(ot => new { Operation = o, Tag = ot.Tag }))
            .GroupBy(x => new { x.Tag.Id, x.Tag.Name, x.Tag.Icon, x.Operation.Money.Currency })
            .Select(g => new CategoryStatsDto
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                TagIcon = g.Key.Icon,
                Amount = g.Sum(x => x.Operation.Money.Amount),
                Currency = g.Key.Currency,
                Count = g.Count(),
                Percentage = 0
            })
            .ToList();

        // Вычисляем проценты
        foreach (var categoryGroup in categories.GroupBy(c => c.Currency))
        {
            var total = categoryGroup.Sum(c => c.Amount);
            foreach (var category in categoryGroup)
            {
                category.Percentage = total > 0 ? (category.Amount / total) * 100 : 0;
            }
        }

        return new CategoryReportDto
        {
            Categories = categories.OrderByDescending(c => c.Amount).ToList()
        };
    }

    public async Task<TrendReportDto> GetTrendReportAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string groupBy = "day")
    {
        var operations = await _context.FinancialOperations
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .ToListAsync();

        List<TrendDataDto> data;

        switch (groupBy.ToLower())
        {
            case "week":
                data = operations
                    .GroupBy(o => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        o.OperationDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .Select(g => new TrendDataDto
                    {
                        Date = g.First().OperationDateTime.Date,
                        Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                        Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                        Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                                 g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;

            case "month":
                data = operations
                    .GroupBy(o => new { o.OperationDateTime.Year, o.OperationDateTime.Month })
                    .Select(g => new TrendDataDto
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                        Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                        Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                                 g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;

            default: // day
                data = operations
                    .GroupBy(o => o.OperationDateTime.Date)
                    .Select(g => new TrendDataDto
                    {
                        Date = g.Key,
                        Income = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount),
                        Expense = g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount),
                        Balance = g.Where(o => o.Type == OperationType.Income).Sum(o => o.Money.Amount) -
                                 g.Where(o => o.Type == OperationType.Expense).Sum(o => o.Money.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;
        }

        return new TrendReportDto
        {
            Data = data,
            GroupBy = groupBy
        };
    }

    public async Task<byte[]> ExportToCsvAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var operations = await _context.FinancialOperations
            .Include(o => o.OperationTags)
                .ThenInclude(ot => ot.Tag)
            .Where(o => o.OwnerId == userId
                && o.OperationDateTime >= startDate
                && o.OperationDateTime <= endDate)
            .OrderBy(o => o.OperationDateTime)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Date,Type,Amount,Currency,Payment Method,Description,Tags");

        foreach (var op in operations)
        {
            var tags = string.Join(";", op.OperationTags.Select(ot => ot.Tag.Name));
            csv.AppendLine($"{op.OperationDateTime:yyyy-MM-dd HH:mm:ss}," +
                          $"{op.Type}," +
                          $"{op.Money.Amount}," +
                          $"{op.Money.Currency}," +
                          $"{op.PaymentMethod}," +
                          $"\"{op.Description}\"," +
                          $"\"{tags}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToExcelAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        // TODO: Реализовать экспорт в Excel используя EPPlus или ClosedXML
        // Для примера возвращаем CSV
        return await ExportToCsvAsync(userId, startDate, endDate);
    }
}

// ============================================
// AuthService
// ============================================
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IUserService userService)
    {
        _context = context;
        _configuration = configuration;
        _userService = userService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // Ищем пользователя по email
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Неверный email или пароль");

        // Проверяем пароль
        if (!VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный email или пароль");

        // Генерируем токены
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        // Обновляем время последнего входа
        await _userService.UpdateLastLoginAsync(user.Id);

        var userDto = await _userService.GetByIdAsync(user.Id);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenLifetimeMinutes()),
            User = userDto
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        // Проверяем существование пользователя
        var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        // Создаём пользователя
        var createUserDto = new CreateUserDto
        {
            Email = dto.Email,
            Password = dto.Password,
            FullName = dto.FullName,
            Phone = dto.Phone
            
        };

        var userDto = await _userService.CreateAsync(createUserDto);

        // Автоматически логиним пользователя
        return await LoginAsync(new LoginRequestDto
        {
            Email = dto.Email,
            Password = dto.Password
        });
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Находим refresh token в базе
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
            throw new UnauthorizedAccessException("Недействительный refresh token");

        // Проверяем, не отозван ли токен
        if (token.RevokedAt.HasValue)
            throw new UnauthorizedAccessException("Токен был отозван");

        // Проверяем срок действия
        if (token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Срок действия токена истёк");

        // Проверяем активность пользователя
        if (!token.User.IsActive)
            throw new UnauthorizedAccessException("Пользователь неактивен");

        // Генерируем новый access token
        var accessToken = GenerateAccessToken(token.User);

        // Генерируем новый refresh token и удаляем старый
        await RevokeTokenAsync(refreshToken);
        var newRefreshToken = await GenerateRefreshTokenAsync(token.UserId);

        var userDto = await _userService.GetByIdAsync(token.UserId);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenLifetimeMinutes()),
            User = userDto
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
            return false;

        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecretKey()));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Собираем claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            new Claim("userId", user.Id.ToString())
        };

        // Добавляем роли
        if (user.UserRoles != null)
        {
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Code));

                // Добавляем права доступа (permissions)
                var permissions = JsonSerializer.Deserialize<List<string>>(userRole.Role.Permissions);
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }
        }

        var token = new JwtSecurityToken(
            issuer: GetJwtIssuer(),
            audience: GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenLifetimeMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays()),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var passwordHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password)));
        return passwordHash == hash;
    }

    private string GetJwtSecretKey() => _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
    private string GetJwtIssuer() => _configuration["Jwt:Issuer"] ?? "FinanceApp.Api";
    private string GetJwtAudience() => _configuration["Jwt:Audience"] ?? "FinanceApp.Client";
    private int GetAccessTokenLifetimeMinutes() => int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"] ?? "15");
    private int GetRefreshTokenLifetimeDays() => int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"] ?? "7");
}