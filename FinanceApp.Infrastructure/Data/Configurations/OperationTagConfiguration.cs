using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.Data.Configurations;

public class OperationTagConfiguration : IEntityTypeConfiguration<OperationTag>
{
    public void Configure(EntityTypeBuilder<OperationTag> builder)
    {
        builder.ToTable("operation_tags");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.TagId)
            .IsRequired()
            .HasColumnName("tag_id");

        builder.Property(e => e.OperationId)
            .IsRequired()
            .HasColumnName("operation_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Связи типа многие-ко-многим
        builder.HasOne(ot => ot.Tag)
            .WithMany(t => t.OperationTags)
            .HasForeignKey(ot => ot.TagId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_operation_tags_tags");

        builder.HasOne(ot => ot.Operation)
            .WithMany(fo => fo.OperationTags)
            .HasForeignKey(ot => ot.OperationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_operation_tags_operations");

        // Уникальный составной индекс
        builder.HasIndex(e => new { e.OperationId, e.TagId })
            .IsUnique()
            .HasDatabaseName("IX_operation_tags_operation_tag");

        builder.HasComment("Связь операций и тегов (многие-ко-многим)");
    }
}