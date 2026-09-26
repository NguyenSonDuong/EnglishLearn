using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using English.Entity.Enums;

namespace EnglishManager.Converters;

/// <summary>
/// Chuyển đổi enum ContextTag sang chuỗi tiếng Việt (đọc từ DescriptionAttribute).
/// </summary>
public class ContextTagToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ContextTag contextTag)
        {
            var fieldInfo = contextTag.GetType().GetField(contextTag.ToString());
            var descAttr = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? contextTag.ToString();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
