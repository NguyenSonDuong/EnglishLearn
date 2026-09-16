namespace English.Entity.DTOs;

/// <summary>
/// Data Transfer Object cho thực thể MeaningExample.
/// </summary>
public class MeaningExampleDto
{
    public Guid Id { get; set; }
    public Guid MeaningId { get; set; }
    public string Sentence_EN { get; set; } = string.Empty;
    public string Sentence_VI { get; set; } = string.Empty;
    public string? HighlightedTarget { get; set; }
}
