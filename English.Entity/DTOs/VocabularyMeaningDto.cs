using English.Entity.Enums;

namespace English.Entity.DTOs;

/// <summary>
/// Data Transfer Object cho thực thể VocabularyMeaning.
/// </summary>
public class VocabularyMeaningDto
{
    public Guid Id { get; set; }
    public Guid VocabularyId { get; set; }
    public WordClass WordClass { get; set; }
    public string Definition_EN { get; set; } = string.Empty;
    public string Definition_VI { get; set; } = string.Empty;
    public ContextTag Context { get; set; } = ContextTag.General;
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<MeaningExampleDto> Examples { get; set; } = new();
}
