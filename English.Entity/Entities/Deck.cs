using System.ComponentModel.DataAnnotations;

namespace English.Entity.Entities;

/// <summary>Bộ thẻ (Deck) chứa các tài liệu học tập.</summary>
public class Deck
{
    /// <summary>Mã định danh bộ thẻ.</summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Tên bộ thẻ.</summary>
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Mô tả bộ thẻ.</summary>
    [MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>Trạng thái kích hoạt.</summary>
    public bool IsActive { get; set; } = true;

    // ── Navigation ──
    /// <summary>Danh sách tài liệu học tập thuộc bộ thẻ này.</summary>
    public ICollection<LearningMaterial> LearningMaterials { get; set; } = new List<LearningMaterial>();
}
