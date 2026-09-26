using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnglishManager.Converters;

/// <summary>
/// Chuyển đổi bool (IsActive) sang màu nền của nút Mode Toggle trong AI Dictionary Tab.
/// true (đang active) → màu tím chính #6750A4, false → màu nhạt #E8DEF8.
/// </summary>
public class ModeBackgroundConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush =
        new((Color)ColorConverter.ConvertFromString("#6750A4"));

    private static readonly SolidColorBrush InactiveBrush =
        new((Color)ColorConverter.ConvertFromString("#E8DEF8"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? ActiveBrush : InactiveBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
