using System.ComponentModel.DataAnnotations;

namespace English.Entity.Entities;

/// <summary>Lịch sử trả lời một câu hỏi.</summary>
public class StudyHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK tới câu hỏi.</summary>
    public Guid QuestionId { get; set; }

    /// <summary>Trả lời đúng hay sai.</summary>
    public bool IsCorrect { get; set; }

    /// <summary>Thời gian phản hồi (milliseconds).</summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>Thời điểm kiểm tra.</summary>
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──
    public Question Question { get; set; } = null!;
}
