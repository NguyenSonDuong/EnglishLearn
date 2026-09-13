using System.ComponentModel.DataAnnotations;
using English.Entity.Enums;

namespace English.Entity.Entities;

/// <summary>Câu hỏi kiểm tra gắn với một tài liệu học tập.</summary>
public class Question
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK tới tài liệu học tập.</summary>
    public Guid LearningMaterialId { get; set; }

    /// <summary>Loại bài kiểm tra.</summary>
    public TestType TestType { get; set; }

    /// <summary>Nội dung câu hỏi / đề bài.</summary>
    [Required]
    [MaxLength(2048)]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Đáp án đúng.</summary>
    [Required]
    [MaxLength(1024)]
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>JSON payload chứa danh sách các lựa chọn.</summary>
    public string? OptionsPayload { get; set; }

    /// <summary>Đường dẫn file audio (local).</summary>
    [MaxLength(512)]
    public string? AudioLocalPath { get; set; }

    /// <summary>Đường dẫn file hình ảnh (local).</summary>
    [MaxLength(512)]
    public string? ImageLocalPath { get; set; }

    // ── Navigation ──
    public LearningMaterial LearningMaterial { get; set; } = null!;
    /// <summary>Bản ghi ôn tập (quan hệ 1-1).</summary>
    public StudyRecord? StudyRecord { get; set; }
    /// <summary>Lịch sử trả lời (quan hệ 1-N).</summary>
    public ICollection<StudyHistory> StudyHistories { get; set; } = new List<StudyHistory>();
}
