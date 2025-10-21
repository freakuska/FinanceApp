using FinanceApp.Dbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.Data.Configurations;

public class FinancialOperationConfiguration : IEntityTypeConfiguration<FinancialOperation>
{
    public void Configure(EntityTypeBuilder<FinancialOperation> builder)
    {
        builder.ToTable("financial_operations");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Аудит
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");

        // Владельцы
        builder.Property(e => e.OwnerId)
            .IsRequired()
            .HasColumnName("owner_id");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasColumnName("created_by");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by");

        // Финансовые данные
        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(20);  // "Income", "Expense", "Transfer"
        // ===== MONEY VALUE OBJECT (добавить сюда!) =====
        builder.OwnsOne(e => e.Money, money =>
        {
            money.WithOwner().HasForeignKey("Id");
            money.HasKey("Id");
            money.Property<Guid>("Id").HasColumnName("id");
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("currency")
                .HasDefaultValue("RUB")
                .IsFixedLength();
        });

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30);  // "Cash", "Card", "BankTransfer"

        builder.Property(e => e.OperationDateTime)
            .IsRequired()
            .HasColumnName("operation_datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Связи с разными поведениями удаления
        builder.HasOne(fo => fo.Owner)
            .WithMany(u => u.OwnedOperations)
            .HasForeignKey(fo => fo.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_financial_operations_owner");

        builder.HasOne(fo => fo.Creator)
            .WithMany(u => u.CreatedOperations)
            .HasForeignKey(fo => fo.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_financial_operations_creator");

        builder.HasOne(fo => fo.Updater)
            .WithMany(u => u.UpdatedOperations)
            .HasForeignKey(fo => fo.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_financial_operations_updater");

        // Query Filter для Soft Delete
        builder.HasQueryFilter(e => e.DeletedAt == null);

        // Составные индексы для аналитики
        builder.HasIndex(e => new { e.OwnerId, e.Type, e.OperationDateTime })
            .HasDatabaseName("IX_operations_owner_type_date");

        // Частичный индекс только для активных записей
        builder.HasIndex(e => new { e.OwnerId, e.OperationDateTime })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("IX_operations_owner_date_active");

        builder.HasComment("Финансовые операции пользователей");
    }
}