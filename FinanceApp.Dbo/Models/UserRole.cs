namespace FinanceApp.Dbo.Models;

// ============================================
// МОДЕЛЬ: РОЛИ ПОЛЬЗОВАТЕЛЯ
// ============================================
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }

    // Навигационные свойства
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
    public virtual User AssignedByUser { get; set; }
}
