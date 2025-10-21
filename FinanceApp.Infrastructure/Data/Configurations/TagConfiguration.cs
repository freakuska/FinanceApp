using FinanceApp.Dbo.Enums;
using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Обязательные поля
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("slug");

        // Enum конвертация
        builder.Property(e => e.Type)
            .IsRequired()  // Обязательно для enum
            .HasColumnName("type")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.Icon)
            .HasMaxLength(50)
            .HasColumnName("icon");

        builder.Property(e => e.Color)
            .HasMaxLength(7)
            .HasColumnName("color");

        // Иерархия
        builder.Property(e => e.ParentId)
            .HasColumnName("parent_id");

        builder.Property(e => e.Level)
            .HasColumnName("level");

        builder.Property(e => e.Path)
            .HasMaxLength(1000)
            .HasColumnName("path");

        // Флаги
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(e => e.OwnerId)
            .HasColumnName("owner_id");

        // Временные метки
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.SortOrder)
            .HasColumnName("sort_order");

        builder.Property(e => e.UsageCount)
            .HasColumnName("usage_count")
            .HasDefaultValue(0);

        builder.Property(e => e.Visibility)
            .HasColumnName("visibility")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(TagVisibility.Private);
    
        // Связи - само ссылающаяся связь для иерархии
        builder.HasOne(t => t.ParentTag)
            .WithMany(t => t.ChildTags)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tags_parent");

        builder.HasOne(t => t.Owner)
            .WithMany(u => u.OwnedTags)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_tags_owner");

        // Индексы для производительности
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("IX_tags_slug");

        builder.HasIndex(e => new { e.Type, e.IsActive })
            .HasDatabaseName("IX_tags_type_active");

        builder.HasIndex(e => e.UsageCount)
            .IsDescending()
            .HasDatabaseName("IX_tags_usage_count_desc");

        builder.HasIndex(e => new { e.OwnerId, e.Visibility })
            .HasDatabaseName("IX_tags_owner_visibility");

        // Full-text search индекс для PostgreSQL
        builder.HasIndex(e => e.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_tags_name_fulltext");

        builder.HasComment("Категории и теги для операций");
    }
}