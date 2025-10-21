using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Таблица
        builder.ToTable("roles");

        // Первичный ключ
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        // Свойства с валидацией
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // JSON колонка
        builder.Property(e => e.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("jsonb");

        // Булевы значения с default
        builder.Property(e => e.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false)
            .IsRequired();

        // Временные метки
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate()
            .IsRequired();

        // Индексы
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_roles_name");

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_roles_code");

        // Комментарии к таблице (PostgreSQL)
        builder.HasComment("Роли пользователей в системе");
    }
}