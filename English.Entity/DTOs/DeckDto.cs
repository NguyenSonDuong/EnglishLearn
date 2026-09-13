namespace English.Entity.DTOs;

public class DeckDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Số lượng tài liệu trong deck (computed).</summary>
    public int MaterialCount { get; set; }
}
