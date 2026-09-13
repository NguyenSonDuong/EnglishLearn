using System.ComponentModel.DataAnnotations;

namespace English.Entity.Entities;

/// <summary>Bản ghi ôn tập theo thuật toán SM-2 (quan hệ 1-1 với Question).</summary>
public class StudyRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK tới câu hỏi (Unique — quan hệ 1-1).</summary>
    public Guid QuestionId { get; set; }

    /// <summary>Số lần đã ôn tập.</summary>
    public int RepetitionCount { get; set; }

    /// <summary>Hệ số dễ (SM-2 Easiness Factor), mặc định 2.5.</summary>
    public double EasinessFactor { get; set; } = 2.5;

    /// <summary>Khoảng cách ôn tập (ngày).</summary>
    public double Interval { get; set; }

    /// <summary>Thời điểm ôn tập tiếp theo.</summary>
    public DateTime NextReviewTime { get; set; }

    /// <summary>Thời điểm kiểm tra lần cuối.</summary>
    public DateTime LastTestedAt { get; set; }

    // ── Navigation ──
    public Question Question { get; set; } = null!;
}
