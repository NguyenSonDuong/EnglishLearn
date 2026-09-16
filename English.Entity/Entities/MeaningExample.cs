namespace English.Entity.Entities;

/// <summary>
/// Thực thể câu ví dụ minh họa gắn với từng ngữ nghĩa cụ thể (Meaning Example).
/// </summary>
public class MeaningExample
{
    public Guid Id { get; set; }
    public Guid MeaningId { get; set; }
    public string Sentence_EN { get; set; } = string.Empty;
    public string Sentence_VI { get; set; } = string.Empty;
    public string? HighlightedTarget { get; set; }

    // ── Navigation Properties ──
    public VocabularyMeaning? Meaning { get; set; }
}
