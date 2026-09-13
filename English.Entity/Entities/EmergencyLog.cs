using System.ComponentModel.DataAnnotations;

namespace English.Entity.Entities;

/// <summary>Nhật ký bỏ qua khẩn cấp (emergency bypass).</summary>
public class EmergencyLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Thời điểm bỏ qua.</summary>
    public DateTime BypassedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Lý do bỏ qua.</summary>
    [MaxLength(1024)]
    public string? Reason { get; set; }

    /// <summary>Đã bị trừ điểm / phạt chưa.</summary>
    public bool IsDeducted { get; set; }
}
