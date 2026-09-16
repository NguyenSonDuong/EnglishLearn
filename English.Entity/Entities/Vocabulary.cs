using English.Entity.Enums;

namespace English.Entity.Entities;

/// <summary>
/// Thực thể gốc từ vựng (Vocabulary) trong cấu trúc Dictionary Cluster.
/// </summary>
public class Vocabulary
{
    public Guid Id { get; set; }
    public string WordText { get; set; } = string.Empty;
    public string? Phonetic_UK { get; set; }
    public string? Phonetic_US { get; set; }
    public string? AudioPath_UK { get; set; }
    public string? AudioPath_US { get; set; }
    public List<string> WordFamily { get; set; } = new();
    public CEFRLevel Level { get; set; } = CEFRLevel.Uncategorized;

    // ── Navigation Properties ──
    public ICollection<VocabularyMeaning> Meanings { get; set; } = new List<VocabularyMeaning>();
}
