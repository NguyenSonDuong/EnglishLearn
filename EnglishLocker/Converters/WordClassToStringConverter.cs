using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using English.Entity.Enums;

namespace EnglishLocker.Converters;

/// <summary>
/// Chuyển đổi enum WordClass sang chuỗi tiếng Việt (đọc từ [Description] attribute).
/// Ví dụ: WordClass.Adjective → "Tính từ".
/// </summary>
public class WordClassToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WordClass wordClass)
        {
            var fieldInfo = wordClass.GetType().GetField(wordClass.ToString());
            var descAttr = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? wordClass.ToString();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
