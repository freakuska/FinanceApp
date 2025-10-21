using FinanceApp.Dbo.Enums;

namespace FinanceApp.Dbo.Models;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public TagType Type { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public Guid? ParentId { get; set; }
    public int? Level { get; set; }
    public string Path { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? SortOrder { get; set; }
    public int UsageCount { get; set; }
    public TagVisibility Visibility { get; set; }

    // Навигационные свойства
    public virtual Tag ParentTag { get; set; }
    public virtual User Owner { get; set; }
    public virtual ICollection<Tag> ChildTags { get; set; }
    public virtual ICollection<OperationTag> OperationTags { get; set; }
}
