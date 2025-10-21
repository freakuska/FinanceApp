using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // Первичный ключ
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Уникальные поля с валидацией
        builder.Property(e => e.Login)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("login");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(512)
            .HasColumnName("password_hash");

        builder.Property(e => e.FullName)
            .HasMaxLength(255)
            .HasColumnName("full_name");

        builder.Property(e => e.Phone)
            .HasMaxLength(20)
            .HasColumnName("phone");

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(1000)
            .HasColumnName("avatar_url");

        // Флаги
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false);

        // Временные метки
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.LastLoginAt)
            .HasColumnName("last_login_at");

        // JSON настройки
        builder.Property(e => e.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        // Составные индексы для оптимизации
        builder.HasIndex(e => e.Login)
            .IsUnique()
            .HasDatabaseName("IX_users_login");

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_users_email");

        builder.HasIndex(e => e.Phone)
            .HasDatabaseName("IX_users_phone");

        builder.HasIndex(e => new { e.IsActive, e.IsVerified })
            .HasDatabaseName("IX_users_active_verified");

        builder.HasIndex(e => e.LastLoginAt)
            .HasDatabaseName("IX_users_last_login");

        // Частичный индекс (только для активных пользователей)
        builder.HasIndex(e => e.Email)
            .HasFilter("is_active = true")
            .HasDatabaseName("IX_users_email_active");

        builder.HasComment("Пользователи системы");
    }
}