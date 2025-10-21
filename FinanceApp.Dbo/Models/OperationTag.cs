namespace FinanceApp.Dbo.Models;
// ============================================
// МОДЕЛЬ: СВЯЗЬ ОПЕРАЦИИ И ТЕГА
// ============================================
public class OperationTag
{
    public Guid Id { get; set; }
    public Guid TagId { get; set; }
    public Guid OperationId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационные свойства
    public virtual Tag Tag { get; set; }
    public virtual FinancialOperation Operation { get; set; }
}
