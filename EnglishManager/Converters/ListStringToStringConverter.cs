using System.Globalization;
using System.Windows.Data;

namespace EnglishManager.Converters;

/// <summary>
/// Chuyển đổi List&lt;string&gt; sang chuỗi phân cách bởi dấu phẩy ", ".
/// </summary>
public class ListStringToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> list)
        {
            return string.Join(", ", list);
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            return str.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
        return new List<string>();
    }
}
