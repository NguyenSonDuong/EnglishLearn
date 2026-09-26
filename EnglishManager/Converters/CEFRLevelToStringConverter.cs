using System.Globalization;
using System.Windows.Data;
using English.Entity.Enums;

namespace EnglishManager.Converters;

/// <summary>
/// Chuyển đổi enum CEFRLevel sang chuỗi hiển thị.
/// </summary>
public class CEFRLevelToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CEFRLevel level)
        {
            return level switch
            {
                CEFRLevel.A1 => "A1 - Người mới",
                CEFRLevel.A2 => "A2 - Cơ bản",
                CEFRLevel.B1 => "B1 - Trung cấp thấp",
                CEFRLevel.B2 => "B2 - Trung cấp cao",
                CEFRLevel.C1 => "C1 - Nâng cao",
                CEFRLevel.C2 => "C2 - Thành thạo",
                CEFRLevel.Uncategorized => "Chưa phân loại",
                _ => level.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
