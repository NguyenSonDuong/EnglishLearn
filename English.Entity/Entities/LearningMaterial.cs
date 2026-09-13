using System.ComponentModel.DataAnnotations;
using English.Entity.Enums;

namespace English.Entity.Entities;

/// <summary>Tài liệu học tập (từ vựng, cụm từ, câu...).</summary>
public class LearningMaterial
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK tới bộ thẻ chứa tài liệu này.</summary>
    public Guid DeckId { get; set; }

    /// <summary>Thuật ngữ / từ vựng.</summary>
    [Required]
    [MaxLength(512)]
    public string Term { get; set; } = string.Empty;

    /// <summary>Nghĩa / định nghĩa.</summary>
    [Required]
    [MaxLength(1024)]
    public string Meaning { get; set; } = string.Empty;

    /// <summary>Phân loại tài liệu.</summary>
    public CategoryType CategoryType { get; set; } = CategoryType.Vocab;

    /// <summary>Nhãn ngữ cảnh (topic, domain...).</summary>
    [MaxLength(256)]
    public string? ContextTag { get; set; }

    /// <summary>Phiên âm IPA.</summary>
    [MaxLength(256)]
    public string? Phonetics { get; set; }

    /// <summary>Câu ví dụ minh hoạ.</summary>
    [MaxLength(2048)]
    public string? ExampleSentence { get; set; }

    // ── Navigation ──
    public Deck Deck { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
