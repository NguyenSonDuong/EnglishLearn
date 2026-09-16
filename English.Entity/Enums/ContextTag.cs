using System.ComponentModel;

namespace English.Entity.Enums;

/// <summary>
/// Ngữ cảnh sử dụng từ vựng (Context Tag).
/// </summary>
public enum ContextTag
{
    [Description("Thông dụng")]
    General = 1,
    [Description("Trang trọng")]
    Formal = 2,
    [Description("Thân mật")]
    Informal = 3,
    [Description("Slang")]
    Slang = 4,
    [Description("Chuyên ngành")]
    Business = 5,
    [Description("Công nghệ thông tin")]
    IT = 6,
    [Description("Y học")]
    Medical = 7
}
