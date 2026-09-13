using English.Entity.Enums;

namespace English.Entity.DTOs;

public class LearningMaterialDto
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public string? ContextTag { get; set; }
    public string? Phonetics { get; set; }
    public string? ExampleSentence { get; set; }
    /// <summary>Số lượng câu hỏi (computed).</summary>
    public int QuestionCount { get; set; }
}
