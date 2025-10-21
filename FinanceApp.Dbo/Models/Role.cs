namespace FinanceApp.Dbo.Models;

// ============================================
// МОДЕЛЬ: РОЛЬ
// ============================================
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Permissions { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Навигационные свойства
    public virtual ICollection<UserRole> UserRoles { get; set; }
}