using English.Entity.Enums;

namespace English.Entity.DTOs;

/// <summary>
/// Data Transfer Object cho thực thể Vocabulary.
/// </summary>
public class VocabularyDto
{
    public Guid Id { get; set; }
    public string WordText { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phonetic_UK { get; set; }
    public string? Phonetic_US { get; set; }
    public string? AudioPath_UK { get; set; }
    public string? AudioPath_US { get; set; }
    public List<string> WordFamily { get; set; } = new();
    public CEFRLevel Level { get; set; } = CEFRLevel.Uncategorized;
    public List<VocabularyMeaningDto> Meanings { get; set; } = new();
}
