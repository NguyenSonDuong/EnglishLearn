using English.Entity.Enums;

namespace English.Entity.Entities;

/// <summary>
/// Thực thể phân cụm ngữ nghĩa của từ vựng (Vocabulary Meaning) theo từ loại và ngữ cảnh.
/// </summary>
public class VocabularyMeaning
{
    public Guid Id { get; set; }
    public Guid VocabularyId { get; set; }
    public WordClass WordClass { get; set; }
    public string Definition_EN { get; set; } = string.Empty;
    public string Definition_VI { get; set; } = string.Empty;
    public ContextTag Context { get; set; } = ContextTag.General;
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();

    // ── Navigation Properties ──
    public Vocabulary? Vocabulary { get; set; }
    public ICollection<MeaningExample> Examples { get; set; } = new List<MeaningExample>();
}
