using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EnglishLocker.Converters;

/// <summary>
/// Converter: CheatCount > 0 → Visible, ngược lại → Collapsed.
/// Dùng để hiển thị dòng cảnh báo gian lận trên UI.
/// </summary>
public class CheatCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count && count > 0)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
