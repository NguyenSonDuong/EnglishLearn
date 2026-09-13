using System.ComponentModel.DataAnnotations;

namespace English.Entity.Entities;

/// <summary>Cấu hình ứng dụng dạng key-value.</summary>
public class AppConfiguration
{
    /// <summary>Khóa cấu hình (Primary Key).</summary>
    [Key]
    [MaxLength(256)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>Giá trị cấu hình.</summary>
    [MaxLength(1024)]
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>Kiểu dữ liệu của giá trị (string, int, bool, json...).</summary>
    [MaxLength(64)]
    public string ValueType { get; set; } = "string";

    /// <summary>Mô tả mục đích của cấu hình.</summary>
    [MaxLength(512)]
    public string? Description { get; set; }
}
