using System.Linq.Expressions;
using FinanceApp.Dbo.Enums;
using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<FinancialOperation> FinancialOperations { get; set; }
    public DbSet<OperationTag> OperationTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Owned<Money>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Глобальные настройки
        ConfigureGlobalSettings(modelBuilder);

        // Seed данные
        SeedData(modelBuilder);
    }

    private void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Глобальный Query Filter для всех сущностей с DeletedAt
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("DeletedAt") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }

        // Настройка каскадного удаления по умолчанию
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Конвенция именования таблиц в snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }
        }
    }

    private static LambdaExpression GetSoftDeleteFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, "DeletedAt");
        var nullConstant = Expression.Constant(null, typeof(DateTime?));
        var comparison = Expression.Equal(property, nullConstant);
        return Expression.Lambda(comparison, parameter);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x)
            ? "_" + x.ToString()
            : x.ToString())).ToLower();
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed данные для ролей
        var roles = new[]
        {
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Суперадминистратор",
                Code = "SUPER_ADMIN",
                Description = "Полный доступ ко всей системе",
                Permissions = "[\"*\"]",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Администратор",
                Code = "ADMIN",
                Description = "Управление пользователями и настройками",
                Permissions = "[\"users.manage\",\"settings.manage\",\"roles.manage\"]",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Менеджер",
                Code = "MANAGER",
                Description = "Просмотр отчетов и аналитики",
                Permissions = "[\"reports.view\",\"analytics.view\",\"operations.view\"]",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Пользователь",
                Code = "USER",
                Description = "Операции с собственными данными",
                Permissions = "[\"operations.own.manage\",\"tags.own.manage\"]",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Role>().HasData(roles);

        // Seed данные для тегов
        var tags = new[]
        {
            new Tag
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                Name = "Зарплата",
                Slug = "salary",
                Type = TagType.Income,
                Icon = "💰",
                Color = "#4CAF50",
                IsSystem = true,
                IsActive = true,
                Visibility = TagVisibility.Public,
                Level = 0,
                Path = "salary",
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                Name = "Продукты",
                Slug = "groceries",
                Type = TagType.Expense,
                Icon = "🛒",
                Color = "#F44336",
                IsSystem = true,
                IsActive = true,
                Visibility = TagVisibility.Public,
                Level = 0,
                Path = "groceries",
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                Name = "Транспорт",
                Slug = "transport",
                Type = TagType.Expense,
                Icon = "🚗",
                Color = "#FF9800",
                IsSystem = true,
                IsActive = true,
                Visibility = TagVisibility.Public,
                Level = 0,
                Path = "transport",
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                Name = "Развлечения",
                Slug = "entertainment",
                Type = TagType.Expense,
                Icon = "🎮",
                Color = "#9C27B0",
                IsSystem = true,
                IsActive = true,
                Visibility = TagVisibility.Public,
                Level = 0,
                Path = "entertainment",
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Tag>().HasData(tags);
    }
}