using English.Entity.Enums;

namespace English.Entity.DTOs;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid LearningMaterialId { get; set; }
    public TestType TestType { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    /// <summary>Danh sách lựa chọn (parsed từ OptionsPayload JSON).</summary>
    public List<string> Options { get; set; } = new();
    public string? AudioLocalPath { get; set; }
    public string? ImageLocalPath { get; set; }

    /// <summary>Alias cho Prompt để tương thích với binding XAML hiện tại.</summary>
    public string Content => Prompt;
}
