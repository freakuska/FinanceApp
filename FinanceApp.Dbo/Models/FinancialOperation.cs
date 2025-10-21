using FinanceApp.Dbo.Enums;

namespace FinanceApp.Dbo.Models;
public class FinancialOperation
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid OwnerId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public OperationType Type { get; set; }
    public string Description { get; set; }
    public string Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime OperationDateTime { get; set; }
    public Money Money { get; set; } = Money.From(0m, "RUB");

    // Навигационные свойства
    public virtual User Owner { get; set; }
    public virtual User Creator { get; set; }
    public virtual User Updater { get; set; }
    public virtual ICollection<OperationTag> OperationTags { get; set; }
}