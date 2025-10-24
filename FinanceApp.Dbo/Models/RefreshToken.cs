namespace FinanceApp.Dbo.Models;

// ============================================
// МОДЕЛЬ: REFRESH TOKEN
// ============================================
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Навигационные свойства
    public virtual User User { get; set; }
}

