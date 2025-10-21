namespace FinanceApp.Dbo.Models;
// ============================================
// МОДЕЛЬ: ПОЛЬЗОВАТЕЛЬ
// ============================================
public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string Settings { get; set; }

    // Навигационные свойства
    public virtual ICollection<UserRole> UserRoles { get; set; }
    public virtual ICollection<Tag> OwnedTags { get; set; }
    public virtual ICollection<FinancialOperation> OwnedOperations { get; set; }
    public virtual ICollection<FinancialOperation> CreatedOperations { get; set; }
    public virtual ICollection<FinancialOperation> UpdatedOperations { get; set; }
}